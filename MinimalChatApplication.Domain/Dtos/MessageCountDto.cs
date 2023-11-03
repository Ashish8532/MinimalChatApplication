using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Dtos
{
    public class MessageCountDto
    {
        public string ReceiverId { get; set; }
        public int MessageCount { get; set; }
        public bool IsRead { get; set; }

        public string UserId { get; set; }
    }
}
