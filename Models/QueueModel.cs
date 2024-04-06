using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StationQueue.Models
{
    public class QueueModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public int OwnerId { get; set; }

        public List<SongModel> Songs { get; set; } = [];

        public DateTime LastSongStarted { get; set; }
    }
}