using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Dtos
{
    public class UpdateProfileDto
    {
        public string UserId { get; set; }
        public string StatusMessage { get; set; }
    }
}
