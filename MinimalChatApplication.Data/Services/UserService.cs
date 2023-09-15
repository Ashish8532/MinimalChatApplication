using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Services
{
    public class UserService : IUserService
    {
        /// <summary>
        /// Manages user accounts and provides user-related operations.
        /// </summary>
        private readonly UserManager<ChatApplicationUser> _userManager;
        /// <summary>
        /// Handles user sign-in and related authentication operations.
        /// </summary>
        private readonly SignInManager<ChatApplicationUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the UserService class with dependencies for user management and sign-in.
        /// </summary>
        /// <param name="userManager">The UserManager for managing user accounts.</param>
        /// <param name="signInManager">The SignInManager for user sign-in functionality.</param>
        public UserService(UserManager<ChatApplicationUser> userManager,
            SignInManager<ChatApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Retrieves a user asynchronously by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// The user object if found, or null if no user matches the provided email address.
        /// </returns>
        public async Task<ChatApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// Creates a new user asynchronously and returns the operation result.
        /// </summary>
        /// <param name="user">The user to be created.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>
        /// A tuple indicating the success status and a collection of error messages (if any).
        /// - If successful, the success status is true, and the error message collection is empty.
        /// - If unsuccessful, the success status is false, and the error messages describe the reasons for failure.
        /// </returns>
        public async Task<(bool success, IEnumerable<string> errors)> CreateUserAsync(ChatApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return (true, Enumerable.Empty<string>());
            }
            else
            {
                var errorMessages = result.Errors.Select(error => error.Description);
                return (false, errorMessages);
            }
        }
    }
}
