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
        Task<IEnumerable<GifData>> GetAllGifDataAsync();


        Task AddGifDataAsync(string gifName);
    }
}
