﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MinimalChatApplication.API.Hubs;
using MinimalChatApplication.Data.Services;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using System.Security.Claims;

namespace MinimalChatApplication.API.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the MessageController class.
        /// </summary>
        /// <param name="messageService">The service responsible for message-related operations.</param>
        public MessageController(IMessageService messageService, 
            IUserService userService,
            IHubContext<ChatHub> chatHub, 
            IMapper mapper)
        {
            _messageService = messageService;
            _userService = userService;
            _chatHub = chatHub;
            _mapper = mapper;
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
        public async Task<IActionResult> PostMessageAsync([FromBody] MessageDto messageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.MessageValidationFailure,
                        Data = null
                    });
                }
                var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (senderId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = HttpStatusMessages.UnauthorizedAccess,
                        Data = null
                    });
                }
                var message = await _messageService.SendMessageAsync(messageDto, senderId);
                if (message?.Id != null)
                {
                    var userChatResponseDto = await _messageService.IncreaseMessageCountAsync(message.SenderId, message.ReceiverId);

                    var user = await _userService.GetUserByIdAsync(message.ReceiverId);

                    // Broadcasts a new message to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveMessage", message);

                    if (user.IsActive)
                    {
                        // Broadcasts message count and chat status to all connected clients using SignalR.
                        await _chatHub.Clients.All.SendAsync("UpdateMessageCount", userChatResponseDto.MessageCount, userChatResponseDto.IsRead, userChatResponseDto.UserId);
                    }
                    return Ok(new ApiResponse<MessageResponseDto>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.MessageSentSuccessfully,
                        Data = message
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.MessageSendingFailure,
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
        public async Task<IActionResult> PutMessageAsync([FromRoute] int messageId, [FromBody] EditMessageDto editMessageDto)
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

                // Update the message content
                var updateResult = await _messageService.EditMessageAsync(messageId, userId, editMessageDto.Content);

                if (updateResult.Succeeded)
                {
                    // Broadcasts an edited message to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveEditedMessage", messageId, editMessageDto.Content);
                    return Ok(new ApiResponse<object>
                    {
                        StatusCode = updateResult.StatusCode,
                        Message = updateResult.Message,
                        Data = null
                    });
                }
                else
                {
                    return StatusCode(updateResult.StatusCode, new ApiResponse<object>
                    {
                        StatusCode = updateResult.StatusCode,
                        Message = updateResult.Message,
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
        public async Task<IActionResult> DeleteMessageAsync([FromRoute] int messageId)
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

                // Delete the message
                var result = await _messageService.DeleteMessageAsync(messageId, userId);

                if (result.Succeeded)
                {
                    var userChatResponseDto = await _messageService.DecreaseMessageCountAsync(result.Data.SenderId, result.Data.ReceiverId);

                    var user = await _userService.GetUserByIdAsync(result.Data.ReceiverId);

                    // Broadcasts a deleted message notification to all connected clients using SignalR.
                    await _chatHub.Clients.All.SendAsync("ReceiveDeletedMessage", messageId);

                    if (user.IsActive)
                    {
                        // Broadcasts message count and chat status to all connected clients using SignalR.
                        await _chatHub.Clients.All.SendAsync("UpdateMessageCount", userChatResponseDto.MessageCount, userChatResponseDto.IsRead, userChatResponseDto.UserId);
                    }

                    return Ok(new ApiResponse<object>
                    {
                        StatusCode = result.StatusCode,
                        Message = result.Message,
                        Data = null
                    });
                }

                return StatusCode(result.StatusCode, new ApiResponse<object>
                {
                    StatusCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
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
        public async Task<IActionResult> RetrieveConversationHistoryAsync([FromQuery] Guid userId, [FromQuery] DateTime? before = null,
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
                        Message = HttpStatusMessages.InvalidRequestParameter,
                        Data = null
                    });
                }
                var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (loggedInUserId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = HttpStatusMessages.UnauthorizedAccess,
                        Data = null
                    });
                }
                if (!before.HasValue)
                {
                    before = DateTime.Now;
                }

                // Call the service method to retrieve conversation history
                var conversationHistory = await _messageService.GetConversationHistoryAsync(
                       loggedInUserId, userId.ToString(), before, count, sort);

                if (conversationHistory != null || conversationHistory.Any())
                {
                    var userLoginStatus = await _userService.GetUserStatusAsync(userId.ToString());
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = HttpStatusMessages.ConversationRetrieved,
                        Data = conversationHistory,
                        IsActive = userLoginStatus
                    });

                }
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = HttpStatusMessages.ConversationNotFound,
                    Data = null
                });

            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError}  {ex.Message}",
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
        public async Task<IActionResult> UpdateChatStatusAsync([FromQuery] string currentUserId, [FromQuery] string? previousUserId)
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

            var result = await _messageService.UpdateChatStatusAsync(userId, currentUserId, previousUserId);
            if (result.Succeeded)
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
