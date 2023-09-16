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

        ///<summary>
        /// Update a message asynchronously in the data source.
        /// </summary>
        /// <param name="message">The message to update.</param>
        Task UpdateMessageAsync(Message message);
    }
}
