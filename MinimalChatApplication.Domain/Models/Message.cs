using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string SenderId { get; set; }
        public ChatApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }
        public ChatApplicationUser Receiver { get; set; }

        // Content field which stores text & emoji
        public string? Content { get; set; }

        // Gif image foreign key
        public int? GifId { get; set; } 
        [ForeignKey("GifId")] 
        public GifData GifData { get; set; }

        public DateTime Timestamp { get; set; }

    }

}
