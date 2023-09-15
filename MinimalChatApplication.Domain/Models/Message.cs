﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } 
    }

}
