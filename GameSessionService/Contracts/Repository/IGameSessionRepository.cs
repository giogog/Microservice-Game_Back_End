using GameSession.Domain.Entities;

namespace GameSession.Contracts;

public interface IGameSessionRepository
{
    Task<List<Session>> GetAllAsync();
    Task<Session> GetByIdAsync(string id);
    Task AddAsync(Session gameSession);
    Task UpdateAsync(string id, Session gameSession);
    Task DeleteAsync(string id);
}
