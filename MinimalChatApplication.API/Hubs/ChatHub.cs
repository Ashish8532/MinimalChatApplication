using Microsoft.AspNetCore.SignalR;

namespace MinimalChatApplication.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, string content)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", content);
        }

        public async Task EditMessage(int messageId, string content)
        {
            // Implement message editing logic
            await Clients.All.SendAsync("ReceiveEditedMessage", messageId, content);
        }

        public async Task DeleteMessage(int messageId)
        {
            // Implement message deletion logic
            await Clients.All.SendAsync("ReceiveDeletedMessage", messageId);
        }
    }
}
