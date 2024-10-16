using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain.Entities;

public class Region
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
}
