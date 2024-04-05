using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationQueue.Models
{
    public class QueueModel
    {
        public int Id { get; set; } 
        public int OwnerId { get; set; }

        public List<SongModel> Songs { get; set; }

        public DateTime LastSongStarted { get; set; }
    }
}