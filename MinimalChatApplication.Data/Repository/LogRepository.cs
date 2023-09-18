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
    public class LogRepository : ILogRepository
    {
        private readonly ChatApplicationDbContext _dbContext; // Replace with your data context

        public LogRepository(ChatApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a log entry to the database asynchronously.
        /// </summary>
        /// <param name="log">The log entry to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddLogAsync(Log logs)
        {
            // Add the log to the database
            _dbContext.Logs.Add(logs);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves logs from the database within a specified time range.
        /// </summary>
        /// <param name="startTime">Start time for log retrieval.</param>
        /// <param name="endTime">End time for log retrieval.</param>
        /// <returns>A list of logs that match the specified time range.</returns>
        /// <exception cref="ApplicationException">Thrown when an error occurs during database access.</exception>
        public async Task<List<Log>> GetLogsAsync(DateTime startTime, DateTime endTime)
        {
            var logs = await _dbContext.Logs.Where(log => log.Timestamp >= startTime && log.Timestamp <= endTime)
                    .ToListAsync();
            if(logs != null && logs.Any())
            {
                return logs;
            }
            return null;
        }
    }
}
