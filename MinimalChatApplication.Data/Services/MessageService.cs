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


        ///<summary>
        /// Get a message by its ID from the repository asynchronously.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>The message if found, or null if not found.</returns>
        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            return await _messageRepository.GetMessageByIdAsync(messageId);
        }


        ///<summary>
        /// Edit a message's content and update it in the repository asynchronously.
        /// </summary>
        /// <param name="message">The message to be edited.</param>
        /// <param name="newContent">The new content for the message.</param>
        public async Task EditMessageAsync(Message message, string newContent)
        {
            message.Content = newContent;
            await _messageRepository.UpdateMessageAsync(message);
        }
    }
}
