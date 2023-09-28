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

        /// <summary>
        /// Edits a message with the given ID, updating its content.
        /// </summary>
        /// <param name="messageId">The ID of the message to edit.</param>
        /// <param name="userId">The ID of the user attempting to edit the message.</param>
        /// <param name="newContent">The updated content for the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        Task<(bool success, int StatusCode, string message)> EditMessageAsync(int messageId, string userId,  string newContent);

        /// <summary>
        /// Deletes a message with the given ID if it exists and if the user is the sender.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <param name="userId">The ID of the user attempting to delete the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        Task<(bool success, int StatusCode, string message)> DeleteMessageAsync(int messageId, string userId);

        /// <summary>
        /// Retrieves the conversation history between the logged-in user and a specific receiver user.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <param name="before">Optional timestamp to filter messages before a specific time.</param>
        /// <param name="count">The number of messages to retrieve.</param>
        /// <param name="sort">The sorting mechanism for messages (asc or desc).</param>
        /// <returns>An IEnumerable of MessageResponseDto containing conversation history.</returns>
        Task<IEnumerable<MessageResponseDto>> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort);


        ///<summary>
        /// Searches for messages containing a specific query in conversations where the user is either the sender or receiver.
        ///</summary>
        ///<param name="userId">The ID of the user initiating the search.</param>
        ///<param name="query">The string to search for in conversation messages.</param>
        ///<returns>A collection of MessageResponseDto representing the search results.</returns>
        Task<IEnumerable<MessageResponseDto>> SearchConversationsAsync(string userId, string query);
    }
}
