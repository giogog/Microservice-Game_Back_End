using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GameSession.Domain.Entities;

public class KillAction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required string SessionId { get; set; }
    public required Guid KillerId { get; set; }
    public required Guid KilledId { get; set; }
    public required int CurrentRoud { get; set; }
}
