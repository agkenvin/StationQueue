using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StationQueue.Models;

namespace StationQueue.Services.Interfaces
{
    public interface IQueueService
    {
        Task<ServiceResponse<QueueModel>> GetQueue(int id);
        Task<ServiceResponse<QueueModel>> AddSongs(List<SongModel> songs);
        Task<ServiceResponse<QueueModel>> DeleteSongs(List<int> songs);
    }
}