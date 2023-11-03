using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IGifService
    {
        /// <summary>
        /// Retrieves all GIF data asynchronously from the repository.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of GIF data.</returns>
        Task<IEnumerable<GifData>> GetAllGifDataAsync();


        /// <summary>
        /// Adds GIF data with the provided name to the repository and saves the changes.
        /// </summary>
        /// <param name="gifName">The name of the GIF to add to the repository.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddGifDataAsync(string gifName);
    }
}
