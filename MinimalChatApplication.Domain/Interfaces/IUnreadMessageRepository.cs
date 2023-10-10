using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IUnreadMessageRepository: IGenericRepository<UnreadMessageCount>
    {

        ///<summary>
        /// Asynchronously saves all changes made to the database context.
        ///</summary>
        ///<remarks>
        /// Use this method to persist any pending changes to the underlying database.
        /// It ensures that changes are committed atomically and provides a way to handle exceptions.
        ///</remarks>
        Task SaveChangesAsync();

        Task<UnreadMessageCount> GetReceiverMessageChat(string senderId, string receiverId);
        Task<UnreadMessageCount> GetSenderMessageChat(string senderId, string receiverId);
    }
}
