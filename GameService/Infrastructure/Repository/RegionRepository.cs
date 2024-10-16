using Game.Contracts;
using Game.Domain.Entities;
using Game.Infrastructure.Context;
using MongoDB.Driver;

namespace Game.Infrastructure.Repository
{
    public class RegionRepository : IRegionRepository
    {
        private readonly IMongoCollection<Region> _regions;

        public RegionRepository(MongoDbContext context)
        {
            _regions = context.Regions;
        }

        public async Task<List<Region>?> GetAllAsync()
        {
            return await _regions.Find(region => true).ToListAsync();
        }

        public async Task<Region?> GetByIdAsync(string id)
        {
            return await _regions.Find(region => region.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Region region)
        {
            await _regions.InsertOneAsync(region);
        }

        public async Task UpdateAsync(string id, Region region)
        {
            await _regions.ReplaceOneAsync(r => r.Id == id, region);
        }

        public async Task DeleteAsync(string id)
        {
            await _regions.DeleteOneAsync(r => r.Id == id);
        }
    }
}
