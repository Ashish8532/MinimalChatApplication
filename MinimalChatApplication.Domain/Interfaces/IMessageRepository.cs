using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message>
    {

        ///<summary>
        /// Asynchronously saves all changes made to the database context.
        ///</summary>
        ///<remarks>
        /// Use this method to persist any pending changes to the underlying database.
        /// It ensures that changes are committed atomically and provides a way to handle exceptions.
        ///</remarks>
        Task SaveChangesAsync();

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

        /// <summary>
        /// Searches for messages containing a specified query string within conversations of a user (sender or receiver).
        /// </summary>
        /// <param name="userId">ID of the user performing the search.</param>
        /// <param name="query">The string to search within message content.</param>
        /// <returns>A collection of messages matching the search criteria.</returns>
        Task<IEnumerable<Message>> SearchConversationsAsync(string userId, string query);

    }
}
