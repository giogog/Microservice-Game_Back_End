using GameSession.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameSession.Domain.Entities
{
    public class Session
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<Player> Team1 {  get; set; }
        public List<Player> Team2 { get; set; }
        public int[] Score { get; set; } = new int[2] { 0 , 0 };
        public required string Region { get; set; }
        public required string GameMode { get; set; }
        public required int MaxScore { get; set; }
        public int CurrentRound { get; set; } = 0;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; } // Nullable in case the game is ongoing

        public void SetTeams(List<PlayerDto> team1, List<PlayerDto> team2)
        {
            Team1 = team1.Select(p => new Player(p.Id.ToString(), p.UserName, p.Lvl)).ToList();
            Team2 = team2.Select(p => new Player(p.Id.ToString(), p.UserName, p.Lvl)).ToList();
        }

        public List<PlayerDto> GetTeam1()
        {
            return Team1.Select(p => new PlayerDto(Guid.Parse(p.Id), p.UserName, p.Lvl)).ToList();
        }
        public List<PlayerDto> GetTeam2()
        {
            return Team2.Select(p => new PlayerDto(Guid.Parse(p.Id), p.UserName, p.Lvl)).ToList();
        }
    }
    public record Player
    {
        public string Id { get; }
        public string UserName { get; }
        public int Lvl { get; }

        public Player(string id, string userName, int lvl)
        {
            Id = id;
            UserName = userName;
            Lvl = lvl;
        }
    }

}
