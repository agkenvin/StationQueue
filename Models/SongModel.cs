using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationQueue.Models
{
    public class SongModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        // The Duration of the song in seconds 
        public int Duration { get; set; }
    }
}