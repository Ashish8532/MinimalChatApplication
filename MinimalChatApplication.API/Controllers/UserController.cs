using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MinimalChatApplication.API.Hubs;
using MinimalChatApplication.Data.Services;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System.Security.Claims;

namespace MinimalChatApplication.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IMessageService _messageService;

        public UserController(IUserService userService, IConfiguration configuration,
            IHubContext<ChatHub> chatHub, IMessageService messageService)
        {
            _userService = userService;
            _configuration = configuration;
            _chatHub = chatHub;
            _messageService = messageService;
        }

        /// <summary>
        /// Registers a new user via a POST request.
        /// </summary>
        /// <param name="registerDto">The registration data provided by the user.</param>
        /// <returns>
        /// An IActionResult representing the HTTP response:
        /// - 200 OK if registration is successful, along with the registration status and user information.
        /// - 400 Bad Request if there are validation errors in the registration data.
        /// - 409 Conflict if the email is already registered.
        /// - 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto registerDto)
        {
            try
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
                var result = await _userService.RegisterUserAsync(registerDto);

                if (result.success)
                {
                    return StatusCode(result.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = result.StatusCode,
                        Message = result.message,
                        Data = result.userResponseDto
                    });
                }
                else
                {
                    return StatusCode(result.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = result.StatusCode,
                        Message = result.message,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="model">The login data provided by the user.</param>
        /// <returns>
        /// - 200 OK if login is successful, along with a JWT token and user profile details.
        /// - 400 Bad Request if there are validation errors in the provided data.
        /// - 401 Unauthorized if login fails due to incorrect credentials.
        /// - 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Login failed due to validation errors",
                        Data = null
                    });
                }

                var loginResult = await _userService.LoginAsync(model.Email, model.Password);

                if (loginResult.Succeeded)
                {
                    var user = await _userService.GetUserByEmailAsync(model.Email);
                    var jwtToken = _userService.GenerateJwtToken(user);
                    var refreshToken = _userService.GenerateRefreshToken();
                    var refreshTokenValidityInDays = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenValidityInDays"]));

                    await _userService.UpdateRefreshToken(user.Email, refreshToken, refreshTokenValidityInDays);
                 
                    // Broadcasts status to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("UpdateStatus", true, user.Id);

                    return Ok(new
                    {
                        message = loginResult.Message,
                        accessToken = jwtToken,
                        refreshToken,
                        expiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"])),
                        profile = new
                        {
                            id = user.Id,
                            name = user.Name,
                            email = user.Email
                        }
                    });
                }
                else
                {
                    return StatusCode(loginResult.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = loginResult.StatusCode,
                        Message = loginResult.Message,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Retrieves a list of users excluding the current user or returns a message indicating no users found.
        /// </summary>
        /// <returns>
        /// An IActionResult containing user information if users are found, or a message indicating no users found.
        /// </returns>
        /// <remarks>
        /// This method is protected by the [Authorize] attribute and can only be accessed by authenticated users.
        /// It retrieves the unique identifier of the current user and fetches a list of users excluding the current user.
        /// If users are found, their information is returned; otherwise, a message indicating no users found is returned.
        /// </remarks>
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized access",
                        Data = null
                    });
                }

                var users = await _userService.GetUsersExceptCurrentUserAsync(userId);
                

                if (users != null && users.Any())
                {
                    await _messageService.UpdateChatStatusAsync(userId, null, null);
                    return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User list retrieved successfully",
                        Data = users
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "No users found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Handles Google Sign-In by validating a Google ID token and performing user registration or login.
        /// </summary>
        /// <param name="idToken">The Google ID token obtained from the client.</param>
        /// <returns>
        /// An IActionResult indicating the result of the Google Sign-In process.
        /// </returns>
        /// <remarks>
        /// This method accepts a Google ID token as a query parameter and validates it against the configured Google client ID.
        /// If the token is valid, it checks if a user with the associated email already exists.
        /// If the user does not exist, a new user is registered with the provided email and name.
        /// If the registration is successful, a JWT token is generated and returned in the response.
        /// If the user already exists, a JWT token is generated and returned in the response.
        /// If any errors occur during this process, an appropriate error response is returned.
        /// </remarks>
        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignin([FromQuery] string idToken)
        {
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

                var googleClientId = _configuration["Authentication:Google:ClientId"];

                settings.Audience = new List<string>()
                {
                    googleClientId
                };

                GoogleJsonWebSignature.Payload payload = GoogleJsonWebSignature.ValidateAsync(idToken, settings).Result;
                if (payload == null)
                {
                    return BadRequest(new { Message = "Invalid Google Credientials. Invalid Token!" });
                }

                var existingUser = await _userService.GetUserByEmailAsync(payload.Email);

                if (existingUser == null)
                {
                    var registerDto = new RegisterDto
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                    };

                    var result = await _userService.RegisterUserAsync(registerDto);
                    if (result.success)
                    {
                        var user = await _userService.GetUserByEmailAsync(payload.Email);

                        await _userService.UpdateUserStatusAsync(user.Id, true);

                        var jwtToken = _userService.GenerateJwtToken(user);
                        var refreshToken = _userService.GenerateRefreshToken();

                        var refreshTokenValidityInDays = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenValidityInDays"]));

                        await _userService.UpdateRefreshToken(user.Email, refreshToken, refreshTokenValidityInDays);

                        // Broadcasts status to all connected clients using SignalR.
                        await _chatHub.Clients.All.SendAsync("UpdateStatus", true, user.Id);

                        return Ok(new
                        {
                            message = "User login Successfull.",
                            accessToken = jwtToken,
                            refreshToken,
                            expiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"])),
                            profile = new
                            {
                                id = user.Id,
                                name = user.Name,
                                email = user.Email
                            }
                        });
                    }
                    else
                    {
                        return StatusCode(result.StatusCode, new ApiResponse<object>
                        {
                            StatusCode = result.StatusCode,
                            Message = result.message,
                            Data = null
                        });
                    }
                }
                else
                {
                    var user = await _userService.GetUserByEmailAsync(payload.Email);

                    await _userService.UpdateUserStatusAsync(user.Id, true);
                    var jwtToken = _userService.GenerateJwtToken(user);
                    var refreshToken = _userService.GenerateRefreshToken();

                    var refreshTokenValidityInDays = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenValidityInDays"]));

                    await _userService.UpdateRefreshToken(user.Email, refreshToken, refreshTokenValidityInDays);

                    // Broadcasts status to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("UpdateStatus", true, user.Id);

                    return Ok(new
                    {
                        message = "User login Successfull.",
                        accessToken = jwtToken,
                        refreshToken,
                        expiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"])),
                        profile = new
                        {
                            id = user.Id,
                            name = user.Name,
                            email = user.Email
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Refreshes an access token using a valid refresh token.
        /// </summary>
        /// <param name="accessToken">The expired access token.</param>
        /// <param name="refreshToken">The valid refresh token.</param>
        /// <returns>
        /// An IActionResult containing a new access token and refresh token if the request is valid,
        /// or a BadRequest response with an error message if the request is invalid.
        /// </returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromQuery] string accessToken, [FromQuery] string refreshToken)
        {
            try
            {
                if (accessToken == null && refreshToken == null)
                {
                    return BadRequest("Invalid client request");
                }

                var principal = _userService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return BadRequest("Invalid claim principle");
                }

                string username = principal.Identity.Name;

                var user = await _userService.GetUserByEmailAsync(username);

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    return BadRequest("Invalid access token or refresh token");
                }

                var newAccessToken = _userService.GenerateJwtToken(user);
                var newRefreshToken = _userService.GenerateRefreshToken();

                await _userService.UpdateRefreshToken(user.Email, newRefreshToken, null);

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Updates the status of the authenticated user and broadcasts the status to all connected clients using SignalR.
        /// </summary>
        /// <returns>
        /// An IActionResult containing the result of the user status update.
        /// </returns>
        /// <remarks>
        /// This method is protected by the [Authorize] attribute and can only be accessed by authenticated users.
        /// It retrieves the unique identifier of the current user and updates the user status using the UserService.
        /// If the update is successful, it also updates the chat status and broadcasts the new status to connected clients.
        /// </remarks>
        [Authorize]
        [HttpPost("status")]
        public async Task<IActionResult> UpdateUserStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access",
                    Data = null
                });
            }

            var result = await _userService.UpdateUserStatusAsync(userId, false);
            if (result.Success)
            {
                await _messageService.UpdateChatStatusAsync(userId, null, null);

                // Broadcasts status to all connected clients using SignalR.
                await _chatHub.Clients.All.SendAsync("UpdateStatus", false, userId);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = result.Message,
                    Data = null
                });
            }
            else
            {
                switch (result.StatusCode)
                {
                    case StatusCodes.Status404NotFound:
                        return NotFound(new ApiResponse<object>
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = result.Message,
                            Data = null
                        });
                    case StatusCodes.Status400BadRequest:
                        return BadRequest(new ApiResponse<object>
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = result.Message,
                            Data = null
                        });
                    case StatusCodes.Status500InternalServerError:
                        return StatusCode(500, new ApiResponse<object>
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = result.Message,
                            Data = null
                        });
                    default:
                        return StatusCode(500, new ApiResponse<object>
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = result.Message,
                            Data = null
                        });
                }
            }
        }
    }
}
