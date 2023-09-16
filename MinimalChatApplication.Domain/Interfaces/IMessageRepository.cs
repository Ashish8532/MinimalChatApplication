using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IMessageRepository
    {
        /// <summary>
        /// Creates a new message asynchronously and stores it in the database.
        /// </summary>
        /// <param name="message">The message to be created and stored.</param>
        /// <returns>
        /// The unique identifier of the created message.
        /// </returns>
        Task<int?> CreateMessageAsync(Message message);

        ///<summary>
        /// Get a message by its unique identifier asynchronously.
        /// </summary>
        /// <param name="messageId">The unique identifier of the message to retrieve.</param>
        /// <returns>The message with the specified ID, or null if not found.</returns>
        Task<Message> GetMessageByIdAsync(int messageId);

        /// <summary>
        /// Updates a message in the database.
        /// </summary>
        /// <param name="message">The message to be updated.</param>
        /// <returns>True if the message was updated successfully; otherwise, false.</returns>
        Task<bool> UpdateMessageAsync(Message message);

        /// <summary>
        /// Deletes a message from the database.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <returns>True if the message was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteMessageAsync(Message message);
    }
}
