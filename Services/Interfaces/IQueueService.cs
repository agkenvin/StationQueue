using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StationQueue.Models;

namespace StationQueue.Services.Interfaces
{
    public interface IQueueService
    {
        Task<ServiceResponse<QueueModel>> GetQueue(string id);
        Task<ServiceResponse<QueueModel>> CreateQueue();
        Task<ServiceResponse<QueueModel>> AddSongs(string id, List<SongModel> songs);
        Task<ServiceResponse<QueueModel>> DeleteSongs(string id, List<int> songs);
        Task<ServiceResponse<QueueModel>> DeleteQueue(string id);
    }
}