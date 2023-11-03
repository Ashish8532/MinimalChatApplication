using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Services
{
    /// <summary>
    /// Service responsible for managing GIF data in the application.
    /// </summary>
    public class GifService : IGifService
    {
        private readonly IGenericRepository<GifData> _gifRepository;

        public GifService(IGenericRepository<GifData> gifRepository)
        {
            _gifRepository = gifRepository;
        }


        /// <summary>
        /// Retrieves all GIF data asynchronously from the repository.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of GIF data.</returns>
        public async Task<IEnumerable<GifData>> GetAllGifDataAsync()
        {
            return await _gifRepository.GetAllAsync();
        }


        /// <summary>
        /// Adds GIF data with the provided name to the repository and saves the changes.
        /// </summary>
        /// <param name="gifName">The name of the GIF to add to the repository.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddGifDataAsync(string gifName)
        {
            var gifData = new GifData
            {
                GifName = gifName
            };
            await _gifRepository.AddAsync(gifData);
            await _gifRepository.SaveChangesAsync();
        }
    }
}
