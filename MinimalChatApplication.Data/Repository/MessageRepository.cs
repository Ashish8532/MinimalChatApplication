using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Repository
{
    public class MessageRepository: IMessageRepository
    {
        private readonly ChatApplicationDbContext _context;
        /// <summary>
        /// Initializes a new instance of the MessageRepository class.
        /// </summary>
        /// <param name="context">The database context for accessing message data.</param>
        public MessageRepository(ChatApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new message asynchronously and stores it in the database.
        /// </summary>
        /// <param name="message">The message to be created and stored.</param>
        /// <returns>
        /// The unique identifier of the created message.
        /// </returns>
        public async Task<int?> CreateMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return message.Id;
        }


        ///<summary>
        /// Get a message by its unique identifier asynchronously.
        /// </summary>
        /// <param name="messageId">The unique identifier of the message to retrieve.</param>
        /// <returns>The message with the specified ID, or null if not found.</returns>
        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages.FirstOrDefaultAsync(x => x.Id == messageId);
        }


        /// <summary>
        /// Updates a message in the database.
        /// </summary>
        /// <param name="message">The message to be updated.</param>
        /// <returns>True if the message was updated successfully; otherwise, false.</returns>
        public async Task<bool> UpdateMessageAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Deletes a message from the database.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <returns>True if the message was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteMessageAsync(Message message)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
