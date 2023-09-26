using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
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
        public async Task<IEnumerable<ChatApplicationUser>> GetUsers(string currentUserId)
        {
            if (currentUserId == null)
            {
                return null;
            }
            return await _dbContext.Users.Where(u => u.Id != currentUserId).ToListAsync();
        }
    }
}
