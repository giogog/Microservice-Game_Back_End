using Game.Domain.Entities;
using MongoDB.Driver;

namespace Game.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    // Access collections like properties
    public IMongoCollection<Region> Regions => _database.GetCollection<Region>("Regions");
    public IMongoCollection<Gamemode> Gamemodes => _database.GetCollection<Gamemode>("Gamemodes");


}