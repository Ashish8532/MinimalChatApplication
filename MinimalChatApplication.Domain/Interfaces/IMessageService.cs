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
        /// <returns>
        /// A tuple containing an IEnumerable of MessageResponseDto representing the conversation history
        /// and a boolean indicating the status of the receiver user (active or inactive).
        /// </returns>
        Task<(IEnumerable<MessageResponseDto>, bool status)> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort);


        ///<summary>
        /// Searches for messages containing a specific query in conversations where the user is either the sender or receiver.
        ///</summary>
        ///<param name="userId">The ID of the user initiating the search.</param>
        ///<param name="query">The string to search for in conversation messages.</param>
        ///<returns>A collection of MessageResponseDto representing the search results.</returns>
        Task<IEnumerable<MessageResponseDto>> SearchConversationsAsync(string userId, string query);


        /// <summary>
        /// Asynchronously updates the chat status for a user, marking messages as read and managing unread message counts.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the chat status is being updated.</param>
        /// <param name="currentUserId">The ID of the currently active user.</param>
        /// <param name="previousUserId">The ID of the previously active user (optional).</param>
        /// <returns>
        /// A tuple containing the success status, HTTP status code, and a message describing the outcome of the chat status update.
        /// </returns>
        Task<(bool Success, int StatusCode, string Message)> UpdateChatStatusAsync(string userId, string currentUserId, string previousUserId);


        /// <summary>
        /// Asynchronously updates the message count and read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A UserResponseDto containing the updated message count, read status, and logged-in status of the receiver user.
        /// </returns>
        Task<UserResponseDto> UpdateMessageCount(string senderId, string receiverId);
    }
}
