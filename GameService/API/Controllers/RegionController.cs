using API.Controllers;
using Game.Contracts;
using Game.Domain.Entities;
using Game.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Game.API.Controllers
{
    public class RegionController(IRepositoryManager repositoryManager, IEventPublisher eventPublisher) : ApiController(repositoryManager, eventPublisher)
    {
        [Authorize(Roles ="Admin")]
        [HttpPost("{region}")]
        public async Task<IActionResult> AddNewRegion(string region)
        {

            var regions = await repositoryManager.RegionRepository.GetAllAsync();
            if (!regions.Any(r => r.Name == region))
            {
                await repositoryManager.RegionRepository.AddAsync(new Region { Name = region });

                eventPublisher.SendMessage(new RegionCreated(region, "RegionCreated"), "MatchMakingQueue");
            }

            var apiResponse = new ApiResponse("Region add Successfully", true, null, Convert.ToInt32(HttpStatusCode.Created));
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }
    }
}
