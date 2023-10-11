using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MinimalChatApplication.API.Hubs;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using System.Data;
using System.Security.Claims;

namespace MinimalChatApplication.API.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _chatHub;

        /// <summary>
        /// Initializes a new instance of the MessageController class.
        /// </summary>
        /// <param name="messageService">The service responsible for message-related operations.</param>
        public MessageController(IMessageService messageService, IHubContext<ChatHub> chatHub)
        {
            _messageService = messageService;
            _chatHub = chatHub;
        }


        #region Message Operations

        /// <summary>
        /// Sends a message to a user.
        /// </summary>
        /// <param name="messageDto">The message data.</param>
        /// <returns>
        /// Returns a response indicating the message sending status:
        /// - 200 OK if the message is sent successfully, along with the message details.
        /// - 400 Bad Request if there are validation errors.
        /// - 401 Unauthorized if the user is not authorized.
        /// - 500 Internal Server Error if an error occurs during processing.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] MessageDto messageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Message sending failed due to validation errors",
                        Data = null
                    });
                }
                var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (senderId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized access",
                        Data = null
                    });
                }
                var messageId = await _messageService.SendMessageAsync(messageDto, senderId);
                if (messageId != null)
                {
                    var result = await _messageService.UpdateMessageCount(senderId, messageDto.ReceiverId);
                    var messageResponseDto = new MessageResponseDto
                    {
                        MessageId = messageId,
                        SenderId = senderId,
                        ReceiverId = messageDto.ReceiverId,
                        Content = messageDto.Content,
                        Timestamp = DateTime.Now
                    };

                    // Broadcasts a new message to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveMessage", messageResponseDto);

                    if(result.IsLoggedIn)
                    {
                        // Broadcasts message count and chat status to all connected clients using SignalR.
                        await _chatHub.Clients.All.SendAsync("UpdateMessageCount", result.MessageCount, result.IsRead, result.UserId);
                    }
                    return Ok(new ApiResponse<MessageResponseDto>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Message sent successfully",
                        Data = messageResponseDto
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Message sending failed",
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
        /// Edits a message with the given ID, if the user has permission.
        /// </summary>
        /// <param name="messageId">ID of the message to edit.</param>
        /// <param name="editMessageDto">Updated message content.</param>
        /// <returns>
        /// - 200 OK with a success message if the message was edited successfully.
        /// - 400 Bad Request if the message editing failed due to validation errors.
        /// - 401 Unauthorized if the user doesn't have permission.
        /// - 404 Not Found if the message does not exist.
        /// - 500 Internal Server Error if an error occurs during processing.
        /// </returns>
        [HttpPut("{messageId}")]
        public async Task<IActionResult> PutMessage([FromRoute] int messageId, [FromBody] EditMessageDto editMessageDto)
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

                // Update the message content
                var updateResult = await _messageService.EditMessageAsync(messageId, userId, editMessageDto.Content);

                if (updateResult.success)
                {
                    // Broadcasts an edited message to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveEditedMessage", messageId, editMessageDto.Content);
                    return Ok(new ApiResponse<object>
                    {
                        StatusCode = updateResult.StatusCode,
                        Message = updateResult.message,
                        Data = null
                    });
                }
                else
                {
                    return StatusCode(updateResult.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = updateResult.StatusCode,
                        Message = updateResult.message,
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
        /// Deletes a message with the given ID, if the user has permission.
        /// </summary>
        /// <param name="messageId">ID of the message to delete.</param>
        /// <returns>
        /// - 200 OK with a success message if the message was deleted successfully.
        /// - 401 Unauthorized if the user doesn't have permission.
        /// - 404 Not Found if the message does not exist.
        /// - 500 Internal Server Error if an error occurs during processing.
        /// </returns>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] int messageId)
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

                // Delete the message
                var deleteResult = await _messageService.DeleteMessageAsync(messageId, userId);

                if (deleteResult.success)
                {
                    // Broadcasts a deleted message notification to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveDeletedMessage", messageId);
                    return Ok(new ApiResponse<object>
                    {
                        StatusCode = deleteResult.StatusCode,
                        Message = deleteResult.message,
                        Data = null
                    });
                }

                return StatusCode(deleteResult.StatusCode, new ApiResponse<object>
                {
                    StatusCode = deleteResult.StatusCode,
                    Message = deleteResult.message,
                    Data = null
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

        #endregion Message Operations

        #region Retrieve Conversation History

        /// <summary>
        /// Controller method for retrieving the conversation history between the logged-in user and a specific user based on query parameters.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve the conversation with.</param>
        /// <param name="before">Optional timestamp to filter messages before a specific time.</param>
        /// <param name="count">The number of messages to retrieve (default is 20).</param>
        /// <param name="sort">The sorting mechanism for messages (asc or desc) (default is "asc").</param>
        /// <returns>
        /// An IActionResult containing a response with conversation history or an error message.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> RetrieveConversationHistory([FromQuery] Guid userId, [FromQuery] DateTime? before = null,
            [FromQuery] int count = 20, [FromQuery] string sort = "asc")
        {
            try
            {
                // Validate parameters
                if (userId == Guid.Empty || count <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Invalid request parameters",
                        Data = null
                    });
                }
                var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (loggedInUserId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized access",
                        Data = null
                    });
                }
                if (!before.HasValue)
                {
                    before = DateTime.Now;
                }

                // Call the service method to retrieve conversation history
                var (conversationHistory, userStatus) = await _messageService.GetConversationHistoryAsync(
                       loggedInUserId, userId.ToString(), before, count, sort);

                if (conversationHistory != null || conversationHistory.Any())
                {
                    // Broadcasts status to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("UpdateStatus", userStatus);

                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Conversation history retrieved successfully",
                        Data = conversationHistory,
                        IsActive = userStatus
                    });

                }
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Conversation not found",
                    Data = null
                });

            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    Data = null
                });
            }
        }

        #endregion Retrieve Conversation History


        /// <summary>
        /// Controller method for updating the chat status of the logged-in user.
        /// </summary>
        /// <param name="currentUserId">The ID of the user whose chat status is being updated.</param>
        /// <param name="previousUserId">Optional ID of the previously active user.</param>
        /// <returns>
        /// Returns an IActionResult with a response indicating the status of the chat status update:
        /// - 200 OK if the update is successful.
        /// - 401 Unauthorized if the user is not authorized.
        /// - 404 Not Found if the specified user or resource is not found.
        /// - 400 Bad Request if there are validation errors.
        /// - 500 Internal Server Error if an error occurs during processing.
        /// </returns>
        [HttpPost("chat-status")]
        public async Task<IActionResult> UpdateChatStatus([FromQuery] string currentUserId, [FromQuery] string? previousUserId)
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

            var result = await _messageService.UpdateChatStatusAsync(userId, currentUserId, previousUserId);
            if (result.Success)
            {
                // Broadcasts message count and chat status to all connected clients using SignalR.
                await _chatHub.Clients.All.SendAsync("UpdateMessageCount", 0, true, currentUserId);

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
