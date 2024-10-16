using GameSession.Domain.Entities;

namespace GameSession.Contracts;

public interface IKillActionRepository
{
    Task<List<KillAction>> GetAllAsync();
    Task<List<KillAction>> GetActionsBySessionIdAsync(string sessionId);
    Task DeleteBySessionIdAsync(string sessionId);
    Task<KillAction> GetByIdAsync(string id);
    Task AddAsync(KillAction killAction);
    Task UpdateAsync(string id, KillAction killAction);
    Task DeleteAsync(string id);
}
