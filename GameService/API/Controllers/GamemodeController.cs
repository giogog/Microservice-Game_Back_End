using API.Controllers;
using Game.Contracts;
using Game.Domain.Entities;
using Game.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Game.API.Controllers
{
    public class GamemodeController(IRepositoryManager repositoryManager,IEventPublisher eventPublisher):ApiController(repositoryManager,eventPublisher)
    {
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNewRegion([FromBody] AddGamemodeDto gamemodeDto)
        {
            var gamemodes = await repositoryManager.RegionRepository.GetAllAsync();
            if (!gamemodes.Any(r => r.Name == gamemodeDto.Name))
            {
                await repositoryManager.GamemodeRepository.AddAsync(new Gamemode { Name = gamemodeDto.Name, MaxPlayers = gamemodeDto.MaxPlayers, MaxScore = gamemodeDto.MaxScore });

                eventPublisher.SendMessage(new GamemodeCreated(gamemodeDto.Name, gamemodeDto.MaxPlayers, gamemodeDto.MaxScore, "GamemodeCreated"), "MatchMakingQueue");
            }

            var apiResponse = new ApiResponse("Gamemode add Successfully", true, null, Convert.ToInt32(HttpStatusCode.Created));
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }

    }
}
