using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Services
{
    public class GifService : IGifService
    {
        private readonly IGenericRepository<GifData> _gifRepository;

        public GifService(IGenericRepository<GifData> gifRepository)
        {
            _gifRepository = gifRepository;
        }


        public async Task<IEnumerable<GifData>> GetAllGifDataAsync()
        {
            return await _gifRepository.GetAllAsync();
        }


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
