using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface ILogRepository
    {
        /// <summary>
        /// Adds a log entry to the database asynchronously.
        /// </summary>
        /// <param name="log">The log entry to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddLogAsync(Log logs);

        /// <summary>
        /// Retrieves logs from the database within a specified time range.
        /// </summary>
        /// <param name="startTime">Start time for log retrieval.</param>
        /// <param name="endTime">End time for log retrieval.</param>
        /// <returns>A list of logs that match the specified time range.</returns>
        /// <exception cref="ApplicationException">Thrown when an error occurs during database access.</exception>
        Task<List<Log>> GetLogsAsync(DateTime startTime, DateTime endTime);
    }
}
