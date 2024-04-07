using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using StationQueue.Data;
using StationQueue.Models;
using StationQueue.Services.Interfaces;

namespace StationQueue.Services
{
    public class QueueService : IQueueService
    {
        private readonly MongoContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<QueueService> _logger;
        private const string _cachePrefix = "Queue_";
        private TimeSpan _cacheTimeout = TimeSpan.FromMinutes(10);
        public QueueService(MongoContext context, IMemoryCache cache, ILogger<QueueService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ServiceResponse<QueueModel>> GetQueue(string id)
        {
            var serviceResponse = await TryGetQueueFromCache(id);
            if (serviceResponse.Data is not null) //protect from caching nulls
            {
                CacheQueue(id, serviceResponse.Data);
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
            CacheQueue(queue.Id, serviceResponse.Data);
            return serviceResponse;
        }

        //Persist new songs in MongoDB and cache 
        public async Task<ServiceResponse<QueueModel>> AddSongs(string id, List<SongModel> songs)
        {
            var serviceResponse = await TryGetQueueFromCache(id);
            if (serviceResponse.Data is null) { return serviceResponse; }
            if (serviceResponse.Data.OwnerId != getOwnerId())
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"User does not have permission to edit queue with id {id}.";
                return serviceResponse;
            }
            foreach (SongModel song in songs)
            {
                serviceResponse.Data.Songs.Add(song);
            }
            _ = AddSongsToDbAsync(id, songs);
            CacheQueue(id, serviceResponse.Data);
            return serviceResponse;
        }
        //Remove Songs from queue in cache and DB 
        public async Task<ServiceResponse<QueueModel>> DeleteSongs(string id, List<int> songIndices)
        {
            var serviceResponse = await TryGetQueueFromCache(id);
            if (serviceResponse.Data is null) { return serviceResponse; }
            songIndices.Sort(); // sort to be able to use offset below 
            if (serviceResponse.Data.OwnerId != getOwnerId())
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"User does not have permission to edit queue with id {id}.";
                return serviceResponse;
            }
            serviceResponse = RemoveSongsFromQueue(songIndices, serviceResponse.Data);
            if (serviceResponse.Data is null) { return serviceResponse; } //index error when removing songs
            _ = RemoveSongsFromDbAsync(id, serviceResponse.Data); //not awaited

            if (serviceResponse.Data is not null)
            {
                CacheQueue(id, serviceResponse.Data);
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<QueueModel>> DeleteQueue(string id)
        {
            string cacheKey = _cachePrefix + id;
            _cache.Remove(cacheKey);

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
        private async Task<ServiceResponse<QueueModel>> TryGetQueueFromCache(string id)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            string cacheKey = _cachePrefix + id;
            if (_cache.TryGetValue(cacheKey, out QueueModel? queue))
            {
                _logger.LogInformation("Queue retrieved from cache");
                serviceResponse.Data = queue;
            }
            else
            {
                _logger.LogInformation("Queue retrieved from database");
                serviceResponse = await GetQueueAsync(id);
            }
            return serviceResponse;
        }

        private async Task<ServiceResponse<QueueModel>> GetQueueAsync(string id)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            var filter = Builders<QueueModel>.Filter.Eq(u => u.Id, id);
            try
            {
                serviceResponse.Data = await (await _context.Queues.FindAsync(filter)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        private void CacheQueue(string id, QueueModel queue)
        {
            //cache the item for _cachetimeout, extend this every time it is accessed
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_cacheTimeout);
            _cache.Set((_cachePrefix + id), queue, cacheEntryOptions);
        }
        //Persist new songs in MongoDB
        private async Task AddSongsToDbAsync(string id, List<SongModel> songs)
        {
            var filter = Builders<QueueModel>.Filter.And(
                            Builders<QueueModel>.Filter.Eq(u => u.Id, id),
                            Builders<QueueModel>.Filter.Eq(u => u.OwnerId, getOwnerId())
                        );
            var update = Builders<QueueModel>.Update.PushEach<SongModel>("Songs", songs);
            var result = await _context.Queues.UpdateOneAsync(filter, update);
            return;
        }
        //remove songs from the list on the server
        private ServiceResponse<QueueModel> RemoveSongsFromQueue(List<int> songIndices, QueueModel queue)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            for (int i = 0; i < songIndices.Count; i++)
            {
                try
                {
                    queue.Songs.RemoveAt(songIndices[i] - i); //offset by i to account for list changing in size 
                }
                catch (Exception ex)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = ex.Message;
                    return serviceResponse;
                }
            }
            serviceResponse.Data = queue;
            return serviceResponse;
        }
        //remove songs from the list in MongoDb
        private async Task<ServiceResponse<QueueModel>> RemoveSongsFromDbAsync(string id, QueueModel queue)
        {
            var serviceResponse = new ServiceResponse<QueueModel>();
            var filter = Builders<QueueModel>.Filter.And(
                                Builders<QueueModel>.Filter.Eq(u => u.Id, id),
                                Builders<QueueModel>.Filter.Eq(u => u.OwnerId, getOwnerId())
                            );
            var update = Builders<QueueModel>.Update.Set("Songs", queue.Songs);
            var result = await _context.Queues.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Queue with id {id} was not found or this user does not have permission to edit it.";
            }
            else
            {
                serviceResponse.Data = queue;
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