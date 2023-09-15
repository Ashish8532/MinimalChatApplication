using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user asynchronously by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation that, upon completion, returns the user object if found,
        /// or null if no user matches the provided email address.
        /// </returns>
        Task<ChatApplicationUser> GetUserByEmailAsync(string email);

        /// <summary>
        /// Creates a new user asynchronously and returns the operation result.
        /// </summary>
        /// <param name="user">The user to be created.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>
        /// A task representing the asynchronous operation that, upon completion, returns a tuple indicating
        /// the success status (true if successful) and a collection of error messages (if any).
        /// </returns>
        Task<(bool success, IEnumerable<string> errors)> CreateUserAsync(ChatApplicationUser user, string password);
    }
}
