using GameSession.Domain.Entities;
using MongoDB.Driver;

namespace GameSession.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    // Access collections like properties
    public IMongoCollection<Session> GameSessions => _database.GetCollection<Session>("GameSessions");
    public IMongoCollection<KillAction> KillActions => _database.GetCollection<KillAction>("KillActions"); 


}