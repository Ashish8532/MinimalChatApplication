using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Models
{
    public class ChatApplicationUser: IdentityUser
    {
        public string Name { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public bool IsActive { get; set; } = false;

        // To Add Status Message for the particular user.
        public string StatusMessage { get; set; } = "";


        // Add a navigation property for messages sent by this user
        public ICollection<Message> SentMessages { get; set; }

        // Add a navigation property for messages received by this user
        public ICollection<Message> ReceivedMessages { get; set; }

        // Add a navigation property for unread message counts sent by this user
        public ICollection<UnreadMessageCount> SentUnreadMessageCounts { get; set; }

        // Add a navigation property for unread message counts received by this user
        public ICollection<UnreadMessageCount> ReceivedUnreadMessageCounts { get; set; }
    }
}
