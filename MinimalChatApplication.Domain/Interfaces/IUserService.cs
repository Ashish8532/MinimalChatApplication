using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Dtos;
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
        /// Registers a new user asynchronously.
        /// </summary>
        /// <param name="registerDto">The registration data provided by the user.</param>
        /// <returns>
        /// A tuple containing the registration status, HTTP status code, a message indicating the result, and user information.
        /// - success (bool): True if registration is successful; otherwise, false.
        /// - StatusCode (int): The HTTP status code associated with the registration outcome.
        /// - message (string): A message providing details about the registration outcome.
        /// - userResponseDto (UserResponseDto): User information including user ID, name, and email (null if registration failed).
        /// </returns>
        Task<(bool success, int StatusCode, string message, UserResponseDto userResponseDto)> RegisterUserAsync(RegisterDto registerDto);

        /// <summary>
        /// Performs user login asynchronously.
        /// </summary>
        /// <param name="email">The email address of the user attempting to log in.</param>
        /// <param name="password">The user's password for authentication.</param>
        /// <returns>
        /// A tuple containing the login status and a message indicating the result.
        /// - Succeeded (bool): True if the login is successful; otherwise, false.
        /// - Message (string): A message providing details about the login outcome.
        /// </returns>
        Task<(bool Succeeded, int StatusCode, string Message)> LoginAsync(string email, string password);

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
        /// Generates a JWT (JSON Web Token) for the provided user.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <returns>The JWT token as a string.</returns>
        string GenerateJwtToken(ChatApplicationUser user);

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
        Task<IEnumerable<ChatApplicationUser>> GetUsersExceptCurrentUserAsync(string currentUserId);
    }
}
