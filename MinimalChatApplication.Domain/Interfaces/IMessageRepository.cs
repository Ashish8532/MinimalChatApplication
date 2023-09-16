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

        /// <summary>
        /// Retrieves the conversation history between the logged-in user and a specific receiver user from the database.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <param name="before">Optional timestamp to filter messages before a specific time.</param>
        /// <param name="count">The number of messages to retrieve.</param>
        /// <param name="sort">The sorting mechanism for messages (asc or desc).</param>
        /// <returns>An IEnumerable of Message objects representing the conversation history.</returns>
        Task<IEnumerable<Message>> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort);
    }
}
