using Microsoft.AspNetCore.Http;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Generator;

namespace MinimalChatApplication.Data.Services
{
    public class MessageService: IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        /// <summary>
        /// Initializes a new instance of the MessageService class.
        /// </summary>
        /// <param name="messageRepository">The repository responsible for message data access.</param>
        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

       

        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="messageDto">The message data.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <returns>
        /// The unique identifier of the sent message if successful; otherwise, null.
        /// </returns>
        public async Task<int?> SendMessageAsync(MessageDto messageDto, string senderId)
        {
            if(messageDto != null)
            {
                var message = new Message
                {
                    Content = messageDto.Content,
                    SenderId = senderId,
                    ReceiverId = messageDto.ReceiverId,
                    Timestamp = DateTime.UtcNow,
                };
                var messageId = await _messageRepository.CreateMessageAsync(message);
                return messageId;
            }
            return null;
        }


        /// <summary>
        /// Edits a message with the given ID, updating its content.
        /// </summary>
        /// <param name="messageId">The ID of the message to edit.</param>
        /// <param name="userId">The ID of the user attempting to edit the message.</param>
        /// <param name="newContent">The updated content for the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        public async Task<(bool success, int StatusCode, string message)> EditMessageAsync(int messageId, string userId, string newContent)
        {
            // Check if the message with the given ID exists
            var message = await _messageRepository.GetMessageByIdAsync(messageId);

            if (message != null)
            {
                // Check if the user is the sender of the message
                if (message.SenderId != userId)
                {
                    return (false, StatusCodes.Status401Unauthorized, "Unauthorized access");
                }
                if (newContent != null)
                {
                    message.Content = newContent;
                    await _messageRepository.UpdateMessageAsync(message);
                    return (true, StatusCodes.Status200OK, "Message edited successfully");
                }
                else
                {
                    return (false, StatusCodes.Status400BadRequest, "Message editing failed due to validation errors");
                }
            }
            return (false, StatusCodes.Status404NotFound, "Message not found");
        }


        /// <summary>
        /// Deletes a message with the given ID if it exists and if the user is the sender.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <param name="userId">The ID of the user attempting to delete the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        public async Task<(bool success, int StatusCode, string message)> DeleteMessageAsync(int messageId, string userId)
        {
            // Check if the message with the given ID exists
            var message = await _messageRepository.GetMessageByIdAsync(messageId);

            if (message != null)
            {
                // Check if the user is the sender of the message
                if (message.SenderId != userId)
                {
                    return (false, StatusCodes.Status401Unauthorized, "Unauthorized access");
                }
                else
                {
                    await _messageRepository.DeleteMessageAsync(message);
                    return (true, StatusCodes.Status200OK, "Message deleted successfully");
                }
            }
            return (false, StatusCodes.Status404NotFound, "Message not found");
        }
    }
}
