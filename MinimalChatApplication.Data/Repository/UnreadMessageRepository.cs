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
    public class UnreadMessageRepository : GenericRepository<UnreadMessageCount>, IUnreadMessageRepository
    {
        private readonly ChatApplicationDbContext _context;
        public UnreadMessageRepository(ChatApplicationDbContext context) : base(context)
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
        /// Asynchronously retrieves the chat record for the receiver-user and sender-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the receiver-user and sender-user.
        /// </returns>
        public async Task<UnreadMessageCount> GetReceiverMessageChat(string senderId, string receiverId)
        {
            return await _context.UnreadMessageCounts
                 .FirstOrDefaultAsync(x => x.SenderId == receiverId && x.ReceiverId == senderId);
        }


        /// <summary>
        /// Asynchronously retrieves the chat record for the sender-user and receiver-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the sender-user and receiver-user.
        /// </returns>
        public async Task<UnreadMessageCount> GetSenderMessageChat(string senderId, string receiverId)
        {
            return await _context.UnreadMessageCounts
                 .FirstOrDefaultAsync(x => x.SenderId == senderId && x.ReceiverId == receiverId);
        }


        /// <summary>
        /// Asynchronously retrieves all chat records for a logged-in user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains a collection of UnreadMessageCount entities
        /// representing the chat records for the logged-in user.
        /// </returns>
        public async Task<IEnumerable<UnreadMessageCount>> GetAllLoggedInUserChat(string userId)
        {
            return await _context.UnreadMessageCounts.Where(x => x.SenderId == userId).ToListAsync();
        }
    }
}
