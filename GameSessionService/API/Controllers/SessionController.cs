using Game.API.Controllers;
using GameSession.Application.Extension;
using GameSession.Contracts;
using GameSession.Domain.Entities;
using GameSession.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GameSession.API.Controllers
{
    public class SessionController(IRepositoryManager repositoryManager, IGamesessionService gamesessionService) : ApiController(repositoryManager)
    {
        private readonly IGamesessionService _gamesessionService = gamesessionService;


        [HttpPost("kill")]
        public async Task<IActionResult> KillOpponent([FromBody] KillActionDto killActionDto)
        {

            var killAction = new KillAction
            {
                KilledId = killActionDto.KilledId.ToGuid(),
                SessionId = killActionDto.SessionId,
                KillerId = killActionDto.KillerId.ToGuid(),
                CurrentRoud = killActionDto.Currentround
            };


            await _repositoryManager.KillActionRepository.AddAsync(killAction);

            _gamesessionService.InvokeKillAction(new KillActionEventArgs(killActionDto.SessionId, killActionDto.KillerId.ToGuid(), killActionDto.KilledId.ToGuid()));

            var apiResponse = new ApiResponse("Kill saved and event triggered Successfully", true, null, Convert.ToInt32(HttpStatusCode.Created));
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }

        [Authorize]
        [HttpGet("playerstats/{sessionId}")]
        public async Task<ActionResult<Session>> GetSessionPlayerStats(string sessionId)
        {
            var playerId = User.Claims.FirstOrDefault(c => c.Type.Contains("nameid"))?.Value;

            _gamesessionService.InvokePlayerStats(new CheckStatsEventArgs(sessionId, playerId.ToGuid()));

            var apiResponse = new ApiResponse("PlayerDto stats requested", true, null, Convert.ToInt32(HttpStatusCode.OK));
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }
    }
}
