using Game.Contracts;
using Game.Infrastructure.Context;
using Game.Infrastructure.Repository;

namespace Game.Infrastructure;

public class RepositoryManager : IRepositoryManager
{
    private readonly Lazy<IGamemodeRepository> _gameModeRepository;
    private readonly Lazy<IRegionRepository> _regionRepository;
    public RepositoryManager(MongoDbContext mongoDbContext)
    {
        _gameModeRepository = new(() => new GamemodeRepository(mongoDbContext));
        _regionRepository = new(() => new RegionRepository(mongoDbContext));
    } 
    public IGamemodeRepository GamemodeRepository => _gameModeRepository.Value;
    public IRegionRepository RegionRepository => _regionRepository.Value;

}
