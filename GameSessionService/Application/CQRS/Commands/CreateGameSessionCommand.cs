using MediatR;
using GameSession.Domain.Models;

namespace GameSession.Application.MediatR.Commands;

public class CreateGameSessionCommand(List<PlayerDto> players, string region, string gameMode,int maxscore):IRequest
{
    public List<PlayerDto> Players { get; } = players;
    public string Region { get; } = region;
    public string GameMode { get; } = gameMode;
    public int MaxScore { get; } = maxscore;
}
