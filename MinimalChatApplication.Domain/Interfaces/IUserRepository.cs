using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a collection of users excluding the current user or all users if currentUserId is null.
        /// </summary>
        /// <param name="currentUserId">The unique identifier of the current user. Pass null to retrieve all users.</param>
        /// <returns>
        /// A collection of UserResponseDto objects representing users, excluding the current user.
        /// If currentUserId is null, it returns all users available in the database.
        /// </returns>
        /// <remarks>
        /// This method queries the database to retrieve all users except the one identified by the provided currentUserId. 
        /// If currentUserId is null, it returns all users available in the database.
        /// The returned users include additional information such as message count and read status.
        /// </remarks>
        Task<IEnumerable<UserChatResponseDto>> GetUsersAsync(string currentUserId);


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
        Task<bool> GetUserStatusAsync(string userId);
    }
}
