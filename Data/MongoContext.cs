using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StationQueue.Models;

namespace StationQueue.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;
        public IMongoCollection<QueueModel> Queues { get; }

        public MongoContext(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            Queues = _database.GetCollection<QueueModel>(mongoDBSettings.Value.QueueModelCollection);
        }
    }
}