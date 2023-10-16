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


        /// <summary>
        /// Asynchronously retrieves the chat record for the receiver-user and sender-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the receiver-user and sender-user.
        /// </returns>
        Task<UnreadMessageCount> GetReceiverMessageChat(string senderId, string receiverId);


        /// <summary>
        /// Asynchronously retrieves the chat record for the sender-user and receiver-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the sender-user and receiver-user.
        /// </returns>
        Task<UnreadMessageCount> GetSenderMessageChat(string senderId, string receiverId);


        /// <summary>
        /// Asynchronously retrieves all chat records for a logged-in user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains a collection of UnreadMessageCount entities
        /// representing the chat records for the logged-in user.
        /// </returns>
        Task<IEnumerable<UnreadMessageCount>> GetAllLoggedInUserChat(string userId);
    }
}
