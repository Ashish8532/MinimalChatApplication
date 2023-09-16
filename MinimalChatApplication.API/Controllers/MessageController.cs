using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Domain.Dtos;
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

        /// <summary>
        /// Initializes a new instance of the MessageController class.
        /// </summary>
        /// <param name="messageService">The service responsible for message-related operations.</param>
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }


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
                if(messageId != null)
                {
                    var messageResponseDto = new MessageResponseDto
                    {
                        MessageId = messageId,
                        SenderId = senderId,
                        ReceiverId = messageDto.ReceiverId,
                        Content = messageDto.Content,
                        Timestamp = DateTime.UtcNow
                    };

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
                    Message = "An error occurred while processing your request",
                    Data = null
                });
            }
        }


        ///<summary>
        /// Edit a message with the specified ID. Only the sender of the message can edit it.
        /// </summary>
        /// <param name="messageId">ID of the message to edit.</param>
        /// <param name="editMessageDto">DTO containing the updated message content.</param>
        /// <returns>
        /// - 200 OK if the message is successfully edited.
        /// - 400 Bad Request if there are validation errors.
        /// - 401 Unauthorized if the user is not the sender of the message or unauthorized access.
        /// - 404 Not Found if the message with the given ID does not exist.
        /// - 500 Internal Server Error if an error occurs while processing the request.
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

                // Check if the message with the given ID exists
                var message = await _messageService.GetMessageByIdAsync(messageId);

                if (message == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Message not found",
                        Data = null
                    });
                }

                // Check if the user is the sender of the message
                if (message.SenderId != userId)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized access",
                        Data = null
                    });
                }

                // Update the message content
                await _messageService.EditMessageAsync(message, editMessageDto.Content);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Message edited successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while processing your request",
                    Data = null
                });
            }
        }
    }
}
