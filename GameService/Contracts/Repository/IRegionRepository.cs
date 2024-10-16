using Game.Domain.Entities;

namespace Game.Contracts;

public interface IRegionRepository
{
    Task<List<Region>?> GetAllAsync();
    Task<Region?> GetByIdAsync(string id);
    Task AddAsync(Region region);
    Task UpdateAsync(string id, Region region);
    Task DeleteAsync(string id);
}
