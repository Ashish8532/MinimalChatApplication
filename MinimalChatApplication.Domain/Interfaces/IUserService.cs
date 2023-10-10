using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Models;
using System.Security.Claims;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IUserService
    {
        #region User Authentication Operations

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
        /// Generates a secure random string to be used as a refresh token.
        /// </summary>
        /// <returns>A base64-encoded string representing the refresh token.</returns>
        /// <remarks>
        /// This method creates a cryptographically secure random byte array and converts it to a 
        /// base64-encoded string, providing a secure refresh token for user authentication.
        /// </remarks>
        string GenerateRefreshToken();

        /// <summary>
        /// Updates the refresh token for a user identified by the provided email.
        /// </summary>
        /// <param name="email">The email address of the user to update.</param>
        /// <param name="refreshToken">The new refresh token to set for the user.</param>
        /// <param name="refreshTokenValidityInDays">Optional: The new expiration time for the refresh token.</param>
        /// <returns>An IdentityResult indicating the success or failure of the update operation.</returns>
        /// <remarks>
        /// This method updates the refresh token for the user with the specified email. If a new 
        /// expiration time is provided, it updates the refresh token expiry time as well.
        /// </remarks>
        Task<IdentityResult> UpdateRefreshToken(string email, string? refreshToken, DateTime? refreshTokenValidityInDays);

        /// <summary>
        /// Retrieves the claims principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        /// <remarks>
        /// This method validates and extracts the claims principal from an expired JWT token. 
        /// It bypasses audience and issuer validation and disregards token lifetime.
        /// </remarks>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string? token);

        #endregion User Authentication Operations


        ///<summary>
        /// Asynchronously retrieves a list of users except the current user.
        ///</summary>
        ///<param name="currentUserId">The unique identifier of the current user.</param>
        ///<returns>A collection of user entities excluding the current user.</returns>
        Task<IEnumerable<UserResponseDto>> GetUsersExceptCurrentUserAsync(string currentUserId);

        Task<(bool Success, int StatusCode, string Message)> UpdateUserStatusAsync(string loggedInUserId, bool status);
    }
}
