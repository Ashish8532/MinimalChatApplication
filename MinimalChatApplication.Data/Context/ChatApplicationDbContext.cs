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
        public DbSet<Log> Logs { get; set; }
        public DbSet<UnreadMessageCount> UnreadMessageCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships for the Message model
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);


            // Define relationships for the MessageCount model
            modelBuilder.Entity<UnreadMessageCount>()
                .HasOne(mc => mc.Sender)
                .WithMany(u => u.SentUnreadMessageCounts)
                .HasForeignKey(mc => mc.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnreadMessageCount>()
                .HasOne(mc => mc.Receiver)
                .WithMany(u => u.ReceivedUnreadMessageCounts)
                .HasForeignKey(mc => mc.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add unique constraint for UserAId and UserBId in MessageCount
            modelBuilder.Entity<UnreadMessageCount>()
                .HasIndex(mc => new { mc.SenderId, mc.ReceiverId })
                .IsUnique();

        }
    }
}
