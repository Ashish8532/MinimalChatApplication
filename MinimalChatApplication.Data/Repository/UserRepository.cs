using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;

namespace MinimalChatApplication.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserRepository(ChatApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }


        /// <summary>
        /// Retrieves a collection of users excluding the current user or all users if currentUserId is null.
        /// </summary>
        /// <param name="currentUserId">The unique identifier of the current user. Pass null to retrieve all users.</param>
        /// <returns>
        /// A collection of UserChatResponseDto objects representing users, excluding the current user.
        /// If currentUserId is null, it returns all users available in the database.
        /// The returned users include additional information such as message count and read status.
        /// </returns>
        /// <remarks>
        /// This method queries the database to retrieve all users except the one identified by the provided currentUserId. 
        /// If currentUserId is null, it returns all users available in the database.
        /// The returned users include additional information such as message count and read status.
        /// </remarks>
        public async Task<IEnumerable<UserChatResponseDto>> GetUsers(string currentUserId)
        {
            var usersWithMessageCount = await (from user in _dbContext.Users
                                               where user.Id != currentUserId
                                               join unreadMessageCount in _dbContext.UnreadMessageCounts
                                               on new { SenderId = currentUserId, ReceiverId = user.Id }
                                               equals new { unreadMessageCount.SenderId, unreadMessageCount.ReceiverId }
                                               into counts
                                               from count in counts.DefaultIfEmpty()
                                               select new
                                               {
                                                   User = user,
                                                   UnreadMessageCount = count
                                               }).ToListAsync();

            var userChatDtos = _mapper.Map<IEnumerable<UserChatResponseDto>>(usersWithMessageCount.Select(x => x.User));

            foreach (var (userChatDto, count) in userChatDtos.Zip(usersWithMessageCount, (d, c) => (d, c.UnreadMessageCount)))
            {
                userChatDto.MessageCount = count?.MessageCount ?? 0;
                userChatDto.IsRead = count?.IsRead ?? false;
            }

            return userChatDtos;
        }


        /// <summary>
        /// Asynchronously retrieves the status of a user based on the provided user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A boolean indicating the user's status (true for active, false for inactive).
        /// If the user is not found, it returns false.
        /// </returns>
        /// <remarks>
        /// This method queries the database to retrieve the status of the user with the specified userId.
        /// It uses AsNoTracking to avoid attaching the entity to the context.
        /// </remarks>
        public async Task<bool> GetUserStatusAsync(string userId)
        {
            var user = await _dbContext.Users
                .AsNoTracking() // Use AsNoTracking to avoid attaching the entity to the context
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.IsActive ?? false; // Return IsActive or false if user is not found
        }
    }
}
