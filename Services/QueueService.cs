using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using MongoDB.Driver;
using StationQueue.Data;
using StationQueue.Models;
using StationQueue.Services.Interfaces;

namespace StationQueue.Services
{
    public class QueueService : IQueueService
    {

        private readonly MongoContext _context;
        public QueueService(MongoContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<QueueModel>> GetQueue(string id)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            try
            {
                serviceResponse.Data = await GetQueueAsync(id);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<QueueModel>> CreateQueue()
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            QueueModel queue = new QueueModel() { OwnerId = getOwnerId() };
            // Currently auto generating ID on Db otherwise we'd want to handle unique Ids
            await _context.Queues.InsertOneAsync(queue);
            serviceResponse.Data = queue;
            return serviceResponse;
        }

        public async Task<ServiceResponse<QueueModel>> AddSongs(string id, List<SongModel> songs)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            var filter = Builders<QueueModel>.Filter.And(
                Builders<QueueModel>.Filter.Eq(u => u.Id, id),
                Builders<QueueModel>.Filter.Eq(u => u.OwnerId, getOwnerId())
            );
            var update = Builders<QueueModel>.Update.PushEach<SongModel>("Songs", songs);
            var result = await _context.Queues.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Queue with id {id} was not found or this user does not have permission to edit it.";
            }
            else
            {
                serviceResponse.Data = await GetQueueAsync(id);
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<QueueModel>> DeleteSongs(string id, List<int> songIds)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            songIds.Sort(); // sort to be able to use offset below 
            QueueModel? queue = await GetQueueAsync(id);
            for (int i = 0; i < songIds.Count; i++)
            {
                try
                {
                    queue.Songs.RemoveAt(songIds[i] - i); //offset by i to account for list changing in size 
                }
                catch (Exception ex)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = ex.Message;
                    return serviceResponse;
                }
            }
            var filter = Builders<QueueModel>.Filter.And(
                Builders<QueueModel>.Filter.Eq(u => u.Id, id),
                Builders<QueueModel>.Filter.Eq(u => u.OwnerId, getOwnerId())
            );
            var result = await _context.Queues.ReplaceOneAsync(filter, queue);
            if (result.MatchedCount == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Queue with id {id} was not found or this user does not have permission to edit it.";
            }
            else
            {
                serviceResponse.Data = await GetQueueAsync(id);
            }
            return serviceResponse;
        }

        private async Task<QueueModel> GetQueueAsync(string id)
        {
            var filter = Builders<QueueModel>.Filter.Eq(u => u.Id, id);
            QueueModel? queue = await (await _context.Queues.FindAsync(filter)).FirstOrDefaultAsync();
            return queue;
        }

        public async Task<ServiceResponse<QueueModel>> DeleteQueue(string id)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            var filter = Builders<QueueModel>.Filter.And(
                Builders<QueueModel>.Filter.Eq(u => u.Id, id),
                Builders<QueueModel>.Filter.Eq(u => u.OwnerId, getOwnerId())
            );
            var result = await _context.Queues.DeleteOneAsync(filter);
            if (result.DeletedCount == 1)
            {
                serviceResponse.Message = $"Queue with id {id} deleted.";
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Queue with id {id} could not be deleted.";
            }
            return serviceResponse;

        }

         //Placeholder to avoid implementing authentication/authorization
        public int getOwnerId()
        {
            return 1;
        }
    }
}