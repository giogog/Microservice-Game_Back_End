using Game.Contracts;
using Grpc.Core;
using GrpcConfiguration.Gamemode;

namespace Game.Infrastructure.GrpcServer;

public class Gamemodes : GrpcGamemode.GrpcGamemodeBase
{
    private readonly IRepositoryManager _repositoryManager;

    public Gamemodes(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public override async Task<AllGamemodesResponse> GetAllGamemodes(GetAllGamemodesRequest request, ServerCallContext context)
    {
        var response = new AllGamemodesResponse();
        var gamemodes = await _repositoryManager.GamemodeRepository.GetAllAsync();

        foreach (var gamemode in gamemodes)
        {
            response.Gamemodes.Add(new GrpcGamemodeModel
            {
                Name = gamemode.Name,
                Maxplayer = gamemode.MaxPlayers,
                Maxscore = gamemode.MaxScore
            });
        }

        return response;
    }

    public override async Task<GamemodeResponse> GetGamemode(GetGamemodeRequest request, ServerCallContext context)
    {
        var response = new GamemodeResponse();
        var gamemode = await _repositoryManager.GamemodeRepository.GetByExpressionAsync(g=>g.Name == request.Name);

        response.Gamemode = new GrpcGamemodeModel
            {
                Name = gamemode.Name,
                Maxplayer = gamemode.MaxPlayers,
                Maxscore = gamemode.MaxScore
            };
        return response;
    }
}
