using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;

namespace MinimalChatApplication.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Service for managing user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="userService">The service responsible for user-related operations.</param>
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDto">The user registration data.</param>
        /// <returns>
        /// Returns a response indicating the registration status:
        /// - 200 OK if registration is successful, along with the user information.
        /// - 400 Bad Request if there are validation errors.
        /// - 409 Conflict if the email is already registered.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Registration failed due to validation errors",
                    Data = null
                });
            }

            // Check existing user
            var userExists = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return Conflict(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Registration failed because the email is already registered",
                    Data = null
                });
            }
            ChatApplicationUser user = new ChatApplicationUser()
            {
                Email = registerDto.Email,
                PasswordHash = registerDto.Password,
                Name = registerDto.Name,
                UserName = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userService.CreateUserAsync(user, registerDto.Password);
            if (!result.success)
            {
                var message = "Registration failed.";

                foreach (var error in result.errors)
                {
                    message += $" {error}";
                }
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = message,
                    Data = null
                });

            }
            var userResponseDto = new UserResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
            };

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Registration successful",
                Data = userResponseDto
            });
        }
    }
}
