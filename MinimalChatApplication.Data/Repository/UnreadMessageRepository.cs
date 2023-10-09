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

        public async Task<UnreadMessageCount> GetReceiverMessageCount(string senderId, string receiverId)
        {
            return await _context.UnreadMessageCounts
                 .FirstOrDefaultAsync(x => x.SenderId == senderId && x.ReceiverId == receiverId);
        }

        public async Task<UnreadMessageCount> GetSenderMessageCount(string senderId, string receiverId)
        {
            return await _context.UnreadMessageCounts
                 .FirstOrDefaultAsync(x => x.SenderId == receiverId && x.ReceiverId == senderId);
        }
    }
}
