using GameSession.Contracts.Service;
using GameSession.Domain.Entities;
using Grpc.Net.Client;
using GrpcConfiguration.Player;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameSession.Infrastructure.GrpcClient
{
    public class PlayerService : IPlayerService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlayerService> _logger;
        private const int baseExperianceGain = 75;
        private const int baseMoneyGain = 35;

        public PlayerService(IConfiguration configuration, ILogger<PlayerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<bool> UpdatePlayers(List<Player> players,bool isWinner)
        {
            var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
            var client = new GrpcPlayer.GrpcPlayerClient(channel);
            var request = new UpdatePlayersRequest();

            try
            {
                //configure player update
                foreach(var player in players)
                {
                    request.Players.Add(new UpdatePlayersModel { PlayerId = player.Id, Experiance = SetExperianceGain(isWinner), Money = SetMoneyGain(isWinner), Playerlvl = player.Lvl });
                    
                }
                //sending player update
                var response = await client.UpdatePlayersAsync(request);
                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldnot call GRPC Server {ex.Message}");
                return false;
                
            }


        }

        private int SetExperianceGain(bool winner)
        {
            if (winner)
            {
                return baseExperianceGain * 5;
            }
            else
                return baseExperianceGain * 2;
        }

        private int SetMoneyGain(bool winner)
        {
            if (winner)
            {
                return baseMoneyGain * 6;
            }
            else
                return baseMoneyGain * 3;
        }






    }
}
