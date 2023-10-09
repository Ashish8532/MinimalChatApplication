using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Models
{
    public class UnreadMessageCount
    {
        [Key]
        public int Id { get; set; }

        public string SenderId { get; set; }
        public ChatApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }
        public ChatApplicationUser Receiver { get; set; }

        public int MessageCount { get; set; }

        public bool IsRead { get; set; }
    }
}
