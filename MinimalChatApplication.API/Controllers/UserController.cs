using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MinimalChatApplication.API.Hubs;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
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
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IConfiguration configuration,
            IHubContext<ChatHub> chatHub, IMessageService messageService, IMapper mapper)
        {
            _userService = userService;
            _configuration = configuration;
            _chatHub = chatHub;
            _messageService = messageService;
            _mapper = mapper;
        }

        #region User Authentication

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
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.RegistrationFailedValidation,
                        Data = null
                    });
                }
                var result = await _userService.RegisterUserAsync(registerDto);

                if (result.Succeeded)
                {
                    return StatusCode(result.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = result.StatusCode,
                        Message = result.Message,
                        Data = result.Data
                    });
                }
                else
                {
                    return StatusCode(result.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = result.StatusCode,
                        Message = result.Message,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="loginDto">The login data provided by the user.</param>
        /// <returns>
        /// - 200 OK if login is successful, along with a JWT token and user profile details.
        /// - 400 Bad Request if there are validation errors in the provided data.
        /// - 401 Unauthorized if login fails due to incorrect credentials.
        /// - 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.LoginFailedValidation,
                        Data = null
                    });
                }

                var loginResult = await _userService.LoginAsync(loginDto.Email, loginDto.Password);

                if (loginResult.Succeeded)
                {
                    var user = await _userService.GetUserByEmailAsync(loginDto.Email);
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
                        profile = _mapper.Map<UserResponseDto>(user)
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
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
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
        public async Task<IActionResult> GoogleSigninAsync([FromQuery] string idToken)
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
                    return BadRequest(new { Message = HttpStatusMessages.InvalidGoogleCredentials });
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
                    if (result.Succeeded)
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
                            message = HttpStatusMessages.LoginSuccessful,
                            accessToken = jwtToken,
                            refreshToken,
                            expiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"])),
                            profile = _mapper.Map<UserResponseDto>(user)
                        });
                    }
                    else
                    {
                        return StatusCode(result.StatusCode, new ApiResponse<object>
                        {
                            StatusCode = result.StatusCode,
                            Message = result.Message,
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
                        message = HttpStatusMessages.LoginSuccessful,
                        accessToken = jwtToken,
                        refreshToken,
                        expiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:LifetimeInMinutes"])),
                        profile = _mapper.Map<UserResponseDto>(user)
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
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
        public async Task<IActionResult> RefreshTokenAsync([FromQuery] string accessToken, [FromQuery] string refreshToken)
        {
            try
            {
                if (accessToken == null && refreshToken == null)
                {
                    return BadRequest(new { Message = HttpStatusMessages.InvalidClientRequest });
                }

                var principal = _userService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return BadRequest(new { Message = HttpStatusMessages.InvalidClaimPrincipal });
                }

                string username = principal.Identity.Name;

                var user = await _userService.GetUserByEmailAsync(username);

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    return BadRequest(new { Message = HttpStatusMessages.InvalidAccessTokenOrRefreshToken });
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
                    Message = $"{HttpStatusMessages.InternalServerError}  {ex.Message}",
                    Data = null
                });
            }
        }

        #endregion User Authentication


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
        public async Task<IActionResult> GetAllUserAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = HttpStatusMessages.UnauthorizedAccess,
                        Data = null
                    });
                }

                var users = await _userService.GetUsersExceptCurrentUserAsync(userId);


                if (users != null && users.Any())
                {
                    await _messageService.UpdateChatStatusAsync(userId, null, null);
                    return Ok(new ApiResponse<IEnumerable<UserChatResponseDto>>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.UserListRetrievedSuccessfully,
                        Data = users
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = HttpStatusMessages.NoUsersFound,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
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
        public async Task<IActionResult> UpdateUserStatusAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = HttpStatusMessages.UnauthorizedAccess,
                    Data = null
                });
            }

            var result = await _userService.UpdateUserStatusAsync(userId, false);
            if (result.Succeeded)
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


        /// <summary>
        /// Retrieves the profile details of the authenticated user.
        /// </summary>
        /// <returns>
        /// An IActionResult containing the profile details if the user is found, or a "Not Found" response if the user's profile is not found.
        /// </returns>
        /// <remarks>
        /// This method is protected by the [Authorize] attribute and can only be accessed by authenticated users.
        /// It retrieves the unique identifier of the current user and fetches the user's profile details.
        /// If the user is found, their profile details are returned in the response. If the user's profile is not found, a "Not Found" response is returned with an appropriate message.
        /// </remarks>
        [Authorize]
        [HttpGet("profile-details")]
        public async Task<IActionResult> GetProfileDetailsAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = HttpStatusMessages.UnauthorizedAccess,
                        Data = null
                    });
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user != null)
                {
                    var profileDetails = _mapper.Map<UserResponseDto>(user);

                    return Ok(new ApiResponse<UserResponseDto>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.ProfileDetailsRetrieved,
                        Data = profileDetails
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = HttpStatusMessages.ProfileNotFound,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Updates the profile information of the authenticated user.
        /// </summary>
        /// <param name="updateProfileDto">The updated profile information provided by the user.</param>
        /// <returns>
        /// An IActionResult containing the result of the profile update:
        /// - 200 OK if the profile is updated successfully.
        /// - 400 Bad Request if the provided data is invalid.
        /// - 401 Unauthorized if the user is not authorized to perform the update.
        /// - 404 Not Found if the user's profile is not found.
        /// - 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.UpdateProfileValidationFailure,
                        Data = null
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null || userId != updateProfileDto.UserId)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = HttpStatusMessages.UnauthorizedAccess,
                        Data = null
                    });
                }

                var result = await _userService.UpdateUserProfileAsync(updateProfileDto);

                if (result != null)
                {
                    // Broadcast the status message update to connected clients
                    await _chatHub.Clients.All.SendAsync("ReceiveStatusMessageUpdate", userId, result.StatusMessage);

                    return Ok(new ApiResponse<UpdateProfileDto>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.ProfileUpdatedSuccessfullly,
                        Data = result
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = HttpStatusMessages.ProfileUpdationFailed,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}",
                    Data = null
                });
            }
        }

    }
}
