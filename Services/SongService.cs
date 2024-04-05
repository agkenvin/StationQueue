using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StationQueue.Models;

namespace StationQueue.Services
{
    public class SongService : ISongService
    {

        private static List<SongModel> _songs = new List<SongModel> {
                new SongModel {Id = 1, Title = "Shoot a Glance !", Artist = "Hundro", Album = "Shoot a Glance ! - Single", Duration = 116},
                new SongModel {Id = 2, Title = "Another Day in Paradise", Artist = "Quinc XCII", Album = "Change of Scenery - EP ", Duration = 255},
                new SongModel {Id = 3, Title = "Tom's Diner", Artist = "AnnenMayKantereit & Giant Rooks",
                Album = "Tom's Diner - Single", Duration = 269},
                new SongModel {Id = 4, Title = "Gold Snafu", Artist = "Sticky Fingers", Album = "Land of Pleasure", Duration = 219},
                new SongModel {Id = 5, Title = "Feel It Still", Artist = "Portugal the Man", Album = "WoodStock", Duration = 163},
                new SongModel {Id = 6, Title = "YEAH RIGHT", Artist = "Joji", Album = "BALLADS 1", Duration = 174}
           };
        public async Task<ServiceResponse<List<SongModel>>> GetAllSongs()
        {
            var serviceResponse = new ServiceResponse<List<SongModel>>();
            serviceResponse.Data = _songs;
            return serviceResponse;
        }
    }
}