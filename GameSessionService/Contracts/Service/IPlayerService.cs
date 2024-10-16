using GameSession.Domain.Entities;

namespace GameSession.Contracts.Service;

public interface IPlayerService
{
    Task<bool> UpdatePlayers(List<Player> players, bool isWinner);
}
