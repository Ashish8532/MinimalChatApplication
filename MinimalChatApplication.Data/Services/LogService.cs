using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Services
{
    public class LogService : ILogService
    {
        private readonly IGenericRepository<Log> _logRepository;
        public LogService(IGenericRepository<Log> logRepository)
        {
            _logRepository = logRepository;
        }


        /// <summary>
        /// Retrieves logs from the database within a specified time range.
        /// </summary>
        /// <param name="startTime">Start time for log retrieval.</param>
        /// <param name="endTime">End time for log retrieval.</param>
        /// <returns>A list of logs that match the specified time range.</returns>
        /// <exception cref="ApplicationException">Thrown when an error occurs during database access.</exception>
        public async Task<IEnumerable<Log>> GetLogsAsync(DateTime startTime, DateTime endTime)
        {
            Expression<Func<Log, bool>> filter =
                     log => log.Timestamp >= startTime && log.Timestamp <= endTime;

            var logs = await _logRepository.GetByConditionAsync(filter);

            if (logs != null && logs.Any())
            {
                return logs;
            }
            return null;
        }
    }
}
