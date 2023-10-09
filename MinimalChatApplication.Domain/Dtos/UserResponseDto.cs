using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Dtos
{
    public class UserResponseDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int MessageCount { get; set; }
        public bool IsRead { get; set; }
    }
}
