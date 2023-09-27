using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly SignInManager<ChatApplicationUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="userService">The service responsible for user-related operations.</param>
        public UserController(IUserService userService, IConfiguration configuration, SignInManager<ChatApplicationUser> signInManager)
        {
            _userService = userService;
            _configuration = configuration;
            _signInManager = signInManager;

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

                    return Ok(new
                    {
                        message = loginResult.Message,
                        jwtToken,
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
                    var userDtos = users.Select(user => new UserResponseDto
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email
                    }).ToList();

                    return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User list retrieved successfully",
                        Data = userDtos
                    });
                }
                else
                {
                    return Ok(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status200OK,
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
                        var jwtToken = _userService.GenerateJwtToken(user);

                        return Ok(new
                        {
                            message = "User login Successfull.",
                            jwtToken,
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
                    var jwtToken = _userService.GenerateJwtToken(user);

                    return Ok(new
                    {
                        message = "User login Successfull.",
                        jwtToken,
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
    }
}
