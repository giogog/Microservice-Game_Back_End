using GameSession.Contracts;
using GameSession.Domain.Entities;
using GameSession.Infrastructure.Context;
using MongoDB.Driver;

namespace GameSession.Infrastructure.Repository
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly IMongoCollection<Session> _gameSessions;

        public GameSessionRepository(MongoDbContext context)
        {
            _gameSessions = context.GameSessions;  
        }

        public async Task<List<Session>> GetAllAsync()
        {
            return await _gameSessions.Find(session => true).ToListAsync();
        }

        public async Task<Session> GetByIdAsync(string id)
        {
            return await _gameSessions.Find(session => session.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Session gameSession)
        {
            await _gameSessions.InsertOneAsync(gameSession);
        }

        public async Task UpdateAsync(string id, Session gameSession)
        {
            await _gameSessions.ReplaceOneAsync(session => session.Id == id, gameSession);
        }

        public async Task DeleteAsync(string id)
        {
            await _gameSessions.DeleteOneAsync(session => session.Id == id);
        }
    }
}
