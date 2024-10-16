using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Game.Domain.Entities
{
    public class Gamemode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int MaxScore { get; set; }
    }
}
