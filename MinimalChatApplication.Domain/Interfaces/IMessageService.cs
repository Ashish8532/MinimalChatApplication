using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="messageDto">The message data.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <returns>
        /// The unique identifier of the sent message if successful; otherwise, null.
        /// </returns>
        Task<int?> SendMessageAsync(MessageDto messageDto, string senderId);

        ///<summary>
        /// Get a message by its ID from the repository asynchronously.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>The message if found, or null if not found.</returns>
        Task<Message> GetMessageByIdAsync(int messageId);

        ///<summary>
        /// Edit a message's content and update it in the repository asynchronously.
        /// </summary>
        /// <param name="message">The message to be edited.</param>
        /// <param name="newContent">The new content for the message.</param>
        Task EditMessageAsync(Message message, string newContent);
    }
}
