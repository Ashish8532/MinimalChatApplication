using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using System.Security.Claims;

namespace MinimalChatApplication.API.Controllers
{
    [Authorize]
    [Route("api/conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public ConversationController(IMessageService messageService)
        {
            _messageService = messageService;
        }


        ///<summary>
        /// Searches conversations based on the provided query string for the logged-in user.
        /// </summary>
        /// <param name="query">The string used to search in the database of conversations for a message.</param>
        /// <returns>
        /// 200 OK - Conversation searched successfully with a list of matching messages.
        /// 400 Bad Request - Invalid request parameters if no matching conversations found.
        /// 401 Unauthorized - Unauthorized access if the user is not authenticated.
        /// 500 Internal Server Error - An error occurred while processing the request.
        /// </returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchConversationsAsync([FromQuery] string query)
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

                var searchResult = await _messageService.SearchConversationsAsync(userId, query);

                if (searchResult != null && searchResult.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<MessageResponseDto>>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.SearchSuccesssfully,
                        Data = searchResult
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.InvalidRequestParameter,
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
