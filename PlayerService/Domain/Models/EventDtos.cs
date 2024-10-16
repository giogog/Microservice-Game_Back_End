namespace Domain.Models;


public record JoinedMatchMaking(Guid PlayerId,string PlayerName, int Lvl, string Region,string GameMode,string Event);
public record LeftMatchMaking(Guid PlayerId, string PlayerName, string Region, string GameMode, string Event);