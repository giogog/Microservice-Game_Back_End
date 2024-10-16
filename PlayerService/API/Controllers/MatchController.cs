using Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers;

public class MatchController(IEventPublisher eventPublisher, IAuthService authorizationService, IGamemodeService gamemodeService) : ApiController(eventPublisher,authorizationService)
{
    private readonly IGamemodeService _gamemodeService = gamemodeService;

    
    [Authorize]
    [HttpPost("join/{gamemode}")]
    public async Task<IActionResult> JoinMatchMaking(string gamemode)
    {
        var player = await _authService.GetUserWithClaim(User);
    
        var gamemodes = _gamemodeService.GetGamemodes();

        if(!gamemodes.Any(g=>g.Name == gamemode))
        {
            throw new NotFoundException("Gamemode Not Found");
        }

        _eventPublisher.SendMessage(new JoinedMatchMaking(player.Id,player.UserName, player.Level, player.Region,gamemode, "JoinedMatchMaking"), "MatchMakingQueue");

        var apiResponse = new ApiResponse("Player joined MatchMaking", true, null, Convert.ToInt32(HttpStatusCode.OK));
        return StatusCode(apiResponse.StatusCode,apiResponse);
    }
    [Authorize]
    [HttpPost("left/{gamemode}")]
    public async Task<IActionResult> LeftMatchMaking(string gamemode)
    {
        var player = await _authService.GetUserWithClaim(User);

        var gamemodes = _gamemodeService.GetGamemodes();

        if (!gamemodes.Any(g => g.Name == gamemode))
        {
            throw new NotFoundException("Gamemode Not Found");
        }

        _eventPublisher.SendMessage(new LeftMatchMaking(player.Id, player.UserName, player.Region,gamemode, "LeftMatchMaking"), "MatchMakingQueue");

        var apiResponse = new ApiResponse("Player Left MatchMaking", true, null, Convert.ToInt32(HttpStatusCode.OK));
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }
}
