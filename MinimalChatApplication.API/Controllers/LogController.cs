using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;

namespace MinimalChatApplication.API.Controllers
{
    [Route("api/log")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Retrieves logs within a specified time range.
        /// </summary>
        /// <param name="startTime">Optional start time for log retrieval (default: 5 minutes ago).</param>
        /// <param name="endTime">Optional end time for log retrieval (default: current timestamp).</param>
        /// <returns>
        ///   - 200 OK with the list of logs if successful,
        ///   - 404 Not Found if no logs are found within the specified range,
        ///   - 400 Bad Request if there are invalid request parameters or an invalid time range,
        ///   - 401 Unauthorized for unauthorized access,
        ///   - 500 Internal Server Error for other exceptions.
        /// </returns>
        /// <remarks>
        /// This endpoint allows you to retrieve logs recorded within the specified time range.
        /// You can specify the start and end times as query parameters. The default time range
        /// is the last 5 minutes. If no logs are found or if there are invalid request parameters,
        /// appropriate HTTP responses are returned.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetLogsAsync([FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            try
            {
                // Validate and process query parameters
                if (startTime == null)
                    startTime = DateTime.Now.AddMinutes(-5); // Default to 5 minutes ago

                if (endTime == null)
                    endTime = DateTime.Now;

                if (startTime >= endTime)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.InvalidRequestParameter,
                        Data = null
                    });
                }

                // Fetch logs from the repository
                var logs = await _logService.GetLogsAsync(startTime.Value, endTime.Value);

                if (logs == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = HttpStatusMessages.LogNotFound,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<IEnumerable<Log>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = HttpStatusMessages.LogRetrievedSuccessfully,
                    Data = logs
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = HttpStatusMessages.UnauthorizedAccess,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate error responses
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
