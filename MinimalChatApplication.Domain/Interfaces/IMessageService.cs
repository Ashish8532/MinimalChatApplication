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
        Task<MessageResponseDto> SendMessageAsync(MessageDto messageDto, string senderId);

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
        Task<(bool success, int StatusCode, string message, MessageResponseDto deletedMessage)> DeleteMessageAsync(int messageId, string userId);


        /// <summary>
        /// Retrieves the conversation history between a logged-in user and a specific receiver, including user status.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the message receiver.</param>
        /// <param name="before">Optional. Retrieves messages created before this date.</param>
        /// <param name="count">The maximum number of messages to retrieve.</param>
        /// <param name="sort">The sorting order for the retrieved messages.</param>
        /// <returns>
        /// A tuple containing the conversation history as a collection of <see cref="MessageResponseDto"/> and a boolean
        /// indicating the user status of the receiver.
        /// </returns>
        Task<(IEnumerable<MessageResponseDto>, bool status)> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort);


        /// <summary>
        /// Searches for messages containing a specific query in conversations where the user is either the sender or receiver.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the search.</param>
        /// <param name="query">The string to search for in conversation messages.</param>
        /// <returns>
        /// A collection of <see cref="MessageResponseDto"/> representing the search results.
        /// </returns>
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
        /// Asynchronously increases the message count and updates the read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A tuple containing a <see cref="UserChatResponseDto"/> with the updated message count and read status,
        /// and a boolean indicating whether the receiver user is currently logged in.
        /// </returns>
        Task<(UserChatResponseDto userChatResponseDto, bool isLoggedIn)> IncreaseMessageCountAsync(string senderId, string receiverId);


        /// <summary>
        /// Asynchronously decreases the message count and updates the read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A tuple containing a <see cref="UserChatResponseDto"/> with the updated message count and read status,
        /// and a boolean indicating whether the receiver user is currently logged in.
        /// </returns>
        Task<(UserChatResponseDto userChatResponseDto, bool isLoggedIn)> DecreaseMessageCountAsync(string senderId, string receiverId);
    }
}
