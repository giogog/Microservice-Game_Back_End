using GameSession.Contracts;
using GameSession.Domain.Entities;
using GameSession.Infrastructure.Context;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameSession.Infrastructure.Repository
{
    public class KillActionRepository : IKillActionRepository
    {
        private readonly IMongoCollection<KillAction> _killActions;

        public KillActionRepository(MongoDbContext context)
        {
            _killActions = context.KillActions;
        }

        public async Task<List<KillAction>> GetAllAsync()
        {
            return await _killActions.Find(killAction => true).ToListAsync();
        }

        public async Task<KillAction> GetByIdAsync(string id)
        {
            return await _killActions.Find(killAction => killAction.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(KillAction killAction)
        {
            await _killActions.InsertOneAsync(killAction);
        }

        public async Task UpdateAsync(string id, KillAction killAction)
        {
            await _killActions.ReplaceOneAsync(killAction => killAction.Id == id, killAction);
        }

        public async Task DeleteAsync(string id)
        {
            await _killActions.DeleteOneAsync(killAction => killAction.Id == id);
        }

        public async Task<List<KillAction>> GetActionsBySessionIdAsync(string sessionId)
        {
            return await _killActions.Find(killAction => killAction.SessionId == sessionId).ToListAsync();
        }
        public async Task DeleteBySessionIdAsync(string sessionId)
        {
            var filter = Builders<KillAction>.Filter.Eq(k => k.SessionId, sessionId);
            await _killActions.DeleteManyAsync(filter);
        }
    }
}
