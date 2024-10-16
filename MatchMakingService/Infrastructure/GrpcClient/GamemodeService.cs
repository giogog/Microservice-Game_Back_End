using Match.Contracts;
using Match.Domain.Models;
using Grpc.Net.Client;
using GrpcConfiguration.Gamemode;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Match.Infrastructure.GrpcClient;

public class GamemodeService : IGamemodeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GamemodeService> _logger;

    public GamemodeService(IConfiguration configuration, ILogger<GamemodeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public IEnumerable<GamemodeDto>? GetGamemodes()
    {
        var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
        var client = new GrpcGamemode.GrpcGamemodeClient(channel);
        var request = new GetAllGamemodesRequest();


        try
        {
            var reply = client.GetAllGamemodes(request);
            return reply.Gamemodes?.Select(r => new GamemodeDto(r.Name, r.Maxplayer, r.Maxscore));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Couldnot call GRPC Server {ex.Message}");
            return null;
        }





        throw new NotImplementedException();
    }

    public GamemodeDto? GetGamemode(string name)
    {
        var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
        var client = new GrpcGamemode.GrpcGamemodeClient(channel);
        var request = new GetGamemodeRequest()
        {
            Name = name
        };


        try
        {
            var reply = client.GetGamemode(request);

            return new GamemodeDto(reply.Gamemode.Name,reply.Gamemode.Maxplayer,reply.Gamemode.Maxscore);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Couldnot call GRPC Server {ex.Message}");
            return null;
        }
    }
}
