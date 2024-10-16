namespace Game.Contracts;

public interface IRepositoryManager
{
    IGamemodeRepository GamemodeRepository { get; }
    IRegionRepository RegionRepository { get; }

}
