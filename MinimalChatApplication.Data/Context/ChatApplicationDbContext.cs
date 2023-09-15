using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Context
{
    public class ChatApplicationDbContext: IdentityDbContext<ChatApplicationUser>
    {
        public ChatApplicationDbContext(DbContextOptions<ChatApplicationDbContext> options): base(options) 
        {
            
        }

        public DbSet<Message> Messages { get; set; }
    }
}
