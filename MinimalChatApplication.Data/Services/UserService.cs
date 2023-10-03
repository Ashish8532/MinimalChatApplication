  using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            if(registerDto.Password == null)
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
        /// Performs user login asynchronously.
        /// </summary>
        /// <param name="email">The email address of the user attempting to log in.</param>
        /// <param name="password">The user's password for authentication.</param>
        /// <returns>
        /// A tuple containing the login status and a message indicating the result.
        /// - Succeeded (bool): True if the login is successful; otherwise, false.
        /// - Message (string): A message providing details about the login outcome.
        /// </returns>
        public async Task<(bool Succeeded, int StatusCode, string Message)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return (false, StatusCodes.Status401Unauthorized, "Login failed due to incorrect credentials");
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return (true, StatusCodes.Status200OK, "Login successful");
            }
            else
            {
                return (false, StatusCodes.Status401Unauthorized, "Login failed due to incorrect credentials");
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
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Get the JWT secret key from configuration and convert it to a byte array
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // Get the token's lifetime in minutes from configuration
            var lifetimeInMinutes = Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"]);

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


        ///<summary>
        /// Asynchronously retrieves a list of users except the current user.
        ///</summary>
        ///<param name="currentUserId">The unique identifier of the current user.</param>
        ///<returns>A collection of user entities excluding the current user.</returns>
        public async Task<IEnumerable<ChatApplicationUser>> GetUsersExceptCurrentUserAsync(string currentUserId)
        {
           return await _userRepository.GetUsers(currentUserId);
        }

        #endregion User Authentication Operations
    }
}
