using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using StationQueue.Models;
using StationQueue.Services.Interfaces;

namespace StationQueue.Services
{
    public class QueueService : IQueueService
    {
        private static List<QueueModel> _queue = new List<QueueModel>() { new QueueModel { Id = 1, OwnerId = 1, Songs = new List<SongModel>(),
         LastSongStarted = DateTime.Now } };

        public async Task<ServiceResponse<QueueModel>> GetQueue(int id)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            QueueModel? queue = _queue.FirstOrDefault(u => u.Id == id);
            if (queue is null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"No Queue with Id {id} found"; 
            }else{
                serviceResponse.Data = queue;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<QueueModel>> AddSongs(List<SongModel> songs)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            foreach (SongModel song in songs)
            {
                _queue[0].Songs.Add(song);
            }
            serviceResponse.Data = _queue[0];
            return serviceResponse;
        }

        public async Task<ServiceResponse<QueueModel>> DeleteSongs(List<int> songIds)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            songIds.Sort(); // sort to be able to use offset below 
            for (int i = 0; i < songIds.Count; i++)
            {
                try
                {
                    _queue[0].Songs.RemoveAt(songIds[i] - i); //offset by i to account for list changing in size 
                }
                catch (Exception ex)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = ex.Message;
                    return serviceResponse;
                }
            }
            serviceResponse.Data = _queue[0];
            return serviceResponse;
        }

        //Placeholder to avoid implementing authentication/authorization
        public int getOwnerId()
        {
            return 1;
        }
    }
}