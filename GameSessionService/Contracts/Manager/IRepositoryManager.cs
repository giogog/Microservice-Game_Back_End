namespace GameSession.Contracts;

public interface IRepositoryManager
{
    IGameSessionRepository GameSessionRepository { get; }
    IKillActionRepository KillActionRepository { get; }
}
