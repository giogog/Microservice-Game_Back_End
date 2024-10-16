using Contracts;
using Grpc.Core;
using GrpcConfiguration.Player;
using Infrastructure.Context;
using Infrastructure.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.GrpcClient
{
    public class Players:GrpcPlayer.GrpcPlayerBase
    {
        private readonly ILogger<Players> _logger;
        private readonly IRepositoryManager _repositoryManager;

        public Players(ILogger<Players> logger,IRepositoryManager repositoryManager)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
        }

        public override async Task<UpdatePlayersResponse> UpdatePlayers(UpdatePlayersRequest request, ServerCallContext context)
        {
            var response = new UpdatePlayersResponse();
            try
            {
                _logger.LogInformation("Starting updating player data");
                await Parallel.ForEachAsync(request.Players, async (playerModel, token) =>
                {
                    var player = await _repositoryManager.UserRepository.GetUser(p => p.Id == playerModel.PlayerId.ToGuid());

                    if (string.IsNullOrEmpty(player.SecurityStamp))
                    {
                        player.SecurityStamp = Guid.NewGuid().ToString();
                    }

                    player.Money += playerModel.Money;
                    player.Experiance = player.Experiance.CalculateLevelandExperiance(playerModel.Experiance, player.Level).Item2;
                    player.Level = player.Experiance.CalculateLevelandExperiance(playerModel.Experiance, player.Level).Item1;

                    await _repositoryManager.UserRepository.UpdateUser(player);

                });
                return new UpdatePlayersResponse() { Message = "Players Updated Succesfully", Success = true };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }



    }

}
