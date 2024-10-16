using GameSession.Contracts;
using GameSession.Infrastructure.Context;
using GameSession.Infrastructure.Repository;

namespace GameSession.Infrastructure;

public class RepositoryManager : IRepositoryManager
{
    private readonly Lazy<IGameSessionRepository> _gameSessionRepository;
    private readonly Lazy<IKillActionRepository> _killActionRepository;
    public RepositoryManager(MongoDbContext mongoDbContext)
    {
        _gameSessionRepository = new(() => new GameSessionRepository(mongoDbContext));
        _killActionRepository = new(() => new KillActionRepository(mongoDbContext));
    } 
    public IGameSessionRepository GameSessionRepository => _gameSessionRepository.Value;
    public IKillActionRepository KillActionRepository => _killActionRepository.Value;
}
