using Game.Domain.Entities;
using System.Linq.Expressions;

namespace Game.Contracts;

public interface IGamemodeRepository
{
    Task<List<Gamemode>> GetAllAsync();
    Task<Gamemode> GetByExpressionAsync(Expression<Func<Gamemode, bool>> expression);
    Task AddAsync(Gamemode gameMode);
    Task UpdateAsync(string id, Gamemode gameMode);
    Task DeleteAsync(string id);

}
