using Microsoft.AspNetCore.SignalR;
using MinimalChatApplication.Domain.Dtos;

namespace MinimalChatApplication.API.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="messageResponse">The message response DTO to broadcast.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendMessageAsync(MessageResponseDto messageResponse)
        {
            await Clients.All.SendAsync("ReceiveMessage", messageResponse);
        }


        /// <summary>
        /// Broadcasts an edited message to all connected clients.
        /// </summary>
        /// <param name="messageId">The ID of the edited message.</param>
        /// <param name="content">The updated content of the message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditMessageAsync(int messageId, string content)
        {
            await Clients.All.SendAsync("ReceiveEditedMessage", messageId, content);
        }


        /// <summary>
        /// Broadcasts a deleted message to all connected clients.
        /// </summary>
        /// <param name="messageId">The ID of the deleted message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteMessageAsync(int messageId)
        {
            await Clients.All.SendAsync("ReceiveDeletedMessage", messageId);
        }


        /// <summary>
        /// Changes the status of a user and broadcasts the updated status to all clients.
        /// </summary>
        /// <param name="status">The new status to set for the user.</param>
        /// <param name="loggedInUserId">The ID of the user whose status is being changed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task ChangeStatusAsync(bool status, string loggedInUserId)
        {
            await Clients.All.SendAsync("UpdateStatus", status, loggedInUserId);
        }


        /// <summary>
        /// Updates the message count and read status for a user and broadcasts the updated information to all clients.
        /// </summary>
        /// <param name="messageCountDto">The data object containing the new message count, read status, and user ID.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task UpdateMessageCountAndStatusAsync(MessageCountDto messageCountDto)
        {
            await Clients.All.SendAsync("UpdateMessageCount", messageCountDto);
        }


        /// <summary>
        /// Broadcasts a status message update to all clients.
        /// </summary>
        /// <param name="userId">The ID of the user whose status message is updated.</param>
        /// <param name="newStatusMessage">The new status message.</param>
        public async Task StatusMessageUpdate(string userId, string newStatusMessage)
        {
            await Clients.All.SendAsync("ReceiveStatusMessageUpdate", userId, newStatusMessage);
        }
    }
}
