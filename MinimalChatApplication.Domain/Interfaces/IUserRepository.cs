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
        Task<IEnumerable<ChatApplicationUser>> GetUsers(string currentUserId);


        Task<bool> GetUserStatusAsync(string userId);
    }
}
