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
    public class LogRepository : GenericRepository<Log>, ILogRepository
    {
        private readonly ChatApplicationDbContext _dbContext; 

        public LogRepository(ChatApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        ///<summary>
        /// Asynchronously saves all changes made to the database context.
        ///</summary>
        ///<remarks>
        /// Use this method to persist any pending changes to the underlying database.
        /// It ensures that changes are committed atomically and provides a way to handle exceptions.
        ///</remarks>
        public async Task SaveChangesAsync()
        {
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
