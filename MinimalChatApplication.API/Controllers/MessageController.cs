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

                return StatusCode(updateResult.StatusCode, new ApiResponse<object>
                {
                    StatusCode = updateResult.StatusCode,
                    Message = updateResult.message,
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
                    Message = "An error occurred while processing your request",
                    Data = null
                });
            }
        }

        #endregion Message Operations
    }
}
