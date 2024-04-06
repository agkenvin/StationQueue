using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StationQueue.Models;
using StationQueue.Services.Interfaces;

namespace StationQueue.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _queueService;

        public QueueController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<QueueModel>>> GetQueue(string id)
        {
            var response = await _queueService.GetQueue(id);
            if (response.Data is null)
            {
                return NotFound(response);
            }
            else
            {
                return Ok(response);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<QueueModel>>> CreateQueue()
        {
            return Ok(await _queueService.CreateQueue());
        }
        //Take in a list of Songs and append each to the end of the queue 
        [HttpPut]
        [Route("AddSongs")]
        public async Task<ActionResult<ServiceResponse<QueueModel>>> AddSongs(string id, List<SongModel> songs)
        {
            return Ok(await _queueService.AddSongs(id, songs));
        }

        //Take in a list of indices, representing the songs current position in the queue to delete
        [HttpPut]
        [Route("DeleteSongs")]
        public async Task<ActionResult<ServiceResponse<QueueModel>>> DeleteSongs(string id, List<int> songIndices)
        {
            var response = await _queueService.DeleteSongs(id, songIndices);
            if (response.Data is null)
            {
                return NotFound(response);
            }
            else
            {
                return Ok(response);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<QueueModel>>> DeleteQueue(string id)
        {
            var response = await _queueService.DeleteQueue(id);
            if (response.Success == false)
            {
                return NotFound(response);
            }
            else
            {
                return Ok(response);
            }
        }
    }
}