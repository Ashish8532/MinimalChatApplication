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
        public async Task SendMessage(MessageResponseDto messageResponse)
        {
            await Clients.All.SendAsync("ReceiveMessage", messageResponse);
        }


        /// <summary>
        /// Broadcasts an edited message to all connected clients.
        /// </summary>
        /// <param name="messageId">The ID of the edited message.</param>
        /// <param name="content">The updated content of the message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditMessage(int messageId, string content)
        {
            await Clients.All.SendAsync("ReceiveEditedMessage", messageId, content);
        }


        /// <summary>
        /// Broadcasts a deleted message to all connected clients.
        /// </summary>
        /// <param name="messageId">The ID of the deleted message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteMessage(int messageId)
        {
            await Clients.All.SendAsync("ReceiveDeletedMessage", messageId);
        }


        public async Task ChangeStatus(bool status)
        {
            await Clients.All.SendAsync("UpdateStatus", status);
        }

        public async Task UpdateMessageCountAndStatus(int messageCount, bool isRead, string userId)
        {
            await Clients.All.SendAsync("UpdateMessageCount", messageCount, isRead, userId);
        }
    }
}
