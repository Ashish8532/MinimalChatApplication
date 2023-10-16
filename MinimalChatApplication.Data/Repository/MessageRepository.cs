using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Repository
{
    public class MessageRepository: GenericRepository<Message>, IMessageRepository
    {
        private readonly ChatApplicationDbContext _context;
        public MessageRepository(ChatApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        ///<summary>
        /// Asynchronously saves all changes made to the database context.
        ///</summary>
        ///<remarks>
        /// Use this method to persist any pending changes to the underlying database.
        /// It ensures that changes are committed atomically and provides a way to handle exceptions.
        ///</remarks>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the conversation history between the logged-in user and a specific receiver user from the database.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <param name="before">Optional timestamp to filter messages before a specific time.</param>
        /// <param name="count">The number of messages to retrieve.</param>
        /// <param name="sort">The sorting mechanism for messages (asc or desc).</param>
        /// <returns>An IEnumerable of Message objects representing the conversation history.</returns>
        public async Task<IEnumerable<Message>> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort)
        {
            var query = _context.Messages
                .Where(m => (m.SenderId == loggedInUserId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == loggedInUserId));

            if (before.HasValue)
            {
                query = query.Where(m => m.Timestamp < before);
            }

            if (sort.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                query = query.OrderBy(m => m.Timestamp);
            }
            query = query.Take(count);
            query = query.OrderBy(m => m.Id);

            return await query.ToListAsync();
        }


        /// <summary>
        /// Searches for messages containing a specified query string within conversations of a user (sender or receiver).
        /// </summary>
        /// <param name="userId">ID of the user performing the search.</param>
        /// <param name="query">The string to search within message content.</param>
        /// <returns>A collection of messages matching the search criteria.</returns>
        public async Task<IEnumerable<Message>> SearchConversationsAsync(string userId, string query)
        {
            // Implement the logic to search conversations in the database
            var queryResult = await _context.Messages
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && m.Content.Contains(query))
                .ToListAsync();

            return queryResult;
        }

    }
}
