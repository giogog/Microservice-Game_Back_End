using Game.Contracts;
using Game.Domain.Entities;
using Game.Infrastructure.Context;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Game.Infrastructure.Repository;

public class GamemodeRepository : IGamemodeRepository
{
    private readonly IMongoCollection<Gamemode> _gameModes;

    public GamemodeRepository(MongoDbContext context)
    {
        _gameModes = context.Gamemodes;
    }

    public async Task<List<Gamemode>> GetAllAsync()
    {
        return await _gameModes.Find(gameMode => true).ToListAsync();
    }

    public async Task<Gamemode> GetByExpressionAsync(Expression<Func<Gamemode,bool>> expression)
    {
        return await _gameModes.Find(expression).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Gamemode gameMode)
    {
        await _gameModes.InsertOneAsync(gameMode);
    }

    public async Task UpdateAsync(string id, Gamemode gameMode)
    {
        await _gameModes.ReplaceOneAsync(gm => gm.Id == id, gameMode);
    }

    public async Task DeleteAsync(string id)
    {
        await _gameModes.DeleteOneAsync(gm => gm.Id == id);
    }
}
