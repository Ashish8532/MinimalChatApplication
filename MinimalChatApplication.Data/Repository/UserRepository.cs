using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatApplicationDbContext _dbContext;

        public UserRepository(ChatApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a list of users excluding the current user or returns all users if currentUserId is null.
        /// </summary>
        /// <param name="currentUserId">The unique identifier of the current user. Pass null to retrieve all users.</param>
        /// <returns>
        /// A collection of ChatApplicationUser objects representing users, excluding the current user.
        /// If currentUserId is null, it returns all users.
        /// </returns>
        /// <remarks>
        /// This method queries the database to retrieve all users except the one identified by the provided currentUserId. 
        /// If currentUserId is null, it returns all users available in the database.
        /// </remarks>
        public async Task<IEnumerable<UserResponseDto>> GetUsers(string currentUserId)
        {
            var usersWithMessageCount = await (from user in _dbContext.Users
                                               where user.Id != currentUserId
                                               join unreadMessageCount in _dbContext.UnreadMessageCounts
                                               on new { SenderId = user.Id, ReceiverId = currentUserId }
                                               equals new { unreadMessageCount.SenderId, unreadMessageCount.ReceiverId }
                                               into counts
                                               from count in counts.DefaultIfEmpty()
                                               select new UserResponseDto
                                               {
                                                   UserId = user.Id,
                                                   Name = user.Name,
                                                   Email = user.Email,
                                                   MessageCount = count != null ? count.MessageCount : 0,
                                                   IsRead = count != null ? count.IsRead : false
                                               }).ToListAsync();
            return usersWithMessageCount;
        }


        public async Task<bool> GetUserStatusAsync(string userId)
        {
            var user = await _dbContext.Users
                .AsNoTracking() // Use AsNoTracking to avoid attaching the entity to the context
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.IsActive ?? false; // Return IsActive or false if user is not found
        }
    }
}
