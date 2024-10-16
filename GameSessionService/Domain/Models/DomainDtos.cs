using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GameSession.Domain.Models;


public class KillActionEventArgs : EventArgs
{
    public readonly string SessionId;
    public readonly Guid KillerId;
    public readonly Guid KilledId;

    public KillActionEventArgs(string sessionId, Guid killerId, Guid killedId)
    {
        SessionId = sessionId;
        KillerId = killerId;
        KilledId = killedId;
    }
}

public class CheckStatsEventArgs : EventArgs
{
    public readonly string SessionId;
    public readonly Guid PlayerId;

    public CheckStatsEventArgs(string sessionId, Guid playerId)
    {
        SessionId = sessionId;
        PlayerId = playerId;
    }
}

public record KillActionDto(string SessionId, string KillerId, string KilledId, int Currentround);


public record PlayerStatsDto(string TeamName,int Kills,int Deaths);

public class SessionDto
{
    public string Id { get; set; }
    public required Dictionary<Guid, string> Team1 { get; set; }
    public required Dictionary<Guid, string> Team2 { get; set; }
    public int[] Score { get; set; } = new int[2] { 0, 0 };
    public required string Region { get; set; }
    public required string GameMode { get; set; }
    public required int MaxScore { get; set; }
    public int CurrentRound { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; } 

    // Additional fields can be added as needed
}