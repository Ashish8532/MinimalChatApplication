using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MinimalChatApplication.Data.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ChatApplicationUser> _userManager;
        private readonly SignInManager<ChatApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the UserService class with dependencies for user management and sign-in.
        /// </summary>
        /// <param name="userManager">The UserManager for managing user accounts.</param>
        /// <param name="signInManager">The SignInManager for user sign-in functionality.</param>
        public UserService(UserManager<ChatApplicationUser> userManager,
            SignInManager<ChatApplicationUser> signInManager,
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _userRepository = userRepository;
        }

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
        public async Task<(bool success, int StatusCode, string message, UserResponseDto userResponseDto)> RegisterUserAsync(RegisterDto registerDto)
        {
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return (false, StatusCodes.Status409Conflict, "Registration failed because the email is already registered", null);
            }

            var user = new ChatApplicationUser()
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                UserName = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            IdentityResult result;
            if (registerDto.Password == null)
            {
                result = await _userManager.CreateAsync(user);
            }
            else
            {
                result = await _userManager.CreateAsync(user, registerDto.Password);
            }
            if (result.Succeeded)
            {
                var userResponseDto = new UserResponseDto
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                };
                return (true, StatusCodes.Status200OK, "Registration successful", userResponseDto);
            }
            else
            {
                var errorMessages = result.Errors.Select(error => error.Description);
                var message = $"Registration failed: {string.Join(", ", errorMessages)}";
                return (false, StatusCodes.Status400BadRequest, message, null);
            }
        }


        /// <summary>
        /// Asynchronously performs user login with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user attempting to log in.</param>
        /// <param name="password">The user's password for authentication.</param>
        /// <returns>
        /// A tuple containing the login status and a message indicating the result.
        /// - Succeeded (bool): True if the login is successful; otherwise, false.
        /// - StatusCode (int): The HTTP status code indicating the outcome of the login attempt.
        /// - Message (string): A message providing details about the login outcome.
        /// </returns>
        public async Task<(bool Succeeded, int StatusCode, string Message)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return (false, StatusCodes.Status400BadRequest, "Login failed due to incorrect credentials");
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                user.IsActive = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return (false, StatusCodes.Status400BadRequest, "Failed to update user status");
                }
                return (true, StatusCodes.Status200OK, "Login successful");
            }
            else
            {
                return (false, StatusCodes.Status400BadRequest, "Login failed due to incorrect credentials");
            }
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
        /// Generates a JWT token for the specified user with the configured claims and settings.
        /// </summary>
        /// <param name="user">The user for whom the JWT token is generated.</param>
        /// <returns>The JWT token as a string.</returns>
        public string GenerateJwtToken(ChatApplicationUser user)
        {
            // Create a list of authentication claims for the user
            var authClaims = new List<Claim>
            {
                new Claim("Username", user.Name),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Get the JWT secret key from configuration and convert it to a byte array
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // Get the token's lifetime in minutes from configuration
            var lifetimeInMinutes = Convert.ToInt32(_configuration["JWT:TokenValidityInMinutes"]);

            var jwtToken = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(lifetimeInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Serialize the JWT token to a string representation
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }


        /// <summary>
        /// Generates a secure random string to be used as a refresh token.
        /// </summary>
        /// <returns>A base64-encoded string representing the refresh token.</returns>
        /// <remarks>
        /// This method creates a cryptographically secure random byte array and converts it to a 
        /// base64-encoded string, providing a secure refresh token for user authentication.
        /// </remarks>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


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
        public async Task<IdentityResult> UpdateRefreshToken(string email, string? refreshToken, DateTime? refreshTokenValidityInDays)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (refreshTokenValidityInDays == null)
            {
                user.RefreshToken = refreshToken;
                return await _userManager.UpdateAsync(user);
            }
            else
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = refreshTokenValidityInDays;

                return await _userManager.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Retrieves the claims principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        /// <remarks>
        /// This method validates and extracts the claims principal from an expired JWT token. 
        /// It bypasses audience and issuer validation and disregards token lifetime.
        /// </remarks>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        #endregion User Authentication Operations


        ///<summary>
        /// Asynchronously retrieves a list of users except the current user.
        ///</summary>
        ///<param name="currentUserId">The unique identifier of the current user.</param>
        ///<returns>A collection of user entities excluding the current user.</returns>
        public async Task<IEnumerable<UserResponseDto>> GetUsersExceptCurrentUserAsync(string currentUserId)
        {
            return await _userRepository.GetUsers(currentUserId);
        }


        /// <summary>
        /// Asynchronously updates the status of a user based on the provided user ID and status.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the user whose status is being updated.</param>
        /// <param name="status">The new status of the user (true for active, false for inactive).</param>
        /// <returns>
        /// A tuple containing a boolean indicating the success of the operation, the HTTP status code,
        /// and a message describing the outcome of the user status update.
        /// </returns>
        public async Task<(bool Success, int StatusCode, string Message)> UpdateUserStatusAsync(string loggedInUserId, bool status)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null)
                {
                    return (false, StatusCodes.Status404NotFound, "Current user not found");
                }
                else
                {
                    if(status == true)
                    {
                        user.IsActive = true;
                    }
                    else
                    {
                        user.IsActive = false;
                    }
                }
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return (false, StatusCodes.Status400BadRequest, "Failed to update user status");
                }

                return (true, StatusCodes.Status200OK, "User statuses updated successfully");
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                return (false, StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
