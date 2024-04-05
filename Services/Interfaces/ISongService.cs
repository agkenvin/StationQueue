using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StationQueue.Models;

namespace StationQueue.Services
{
    public interface ISongService
    {
        Task<ServiceResponse<List<SongModel>>> GetAllSongs();
    }
}