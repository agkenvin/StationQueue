using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StationQueue.Models;
using StationQueue.Services;

namespace StationQueue.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SongController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<SongModel>>>> Get(){
            return Ok( await _songService.GetAllSongs());
        }

    }
}