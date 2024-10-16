namespace Match.Domain.Models;


public record GenericEventDto(string Event);


public record JoinedMatchMakingDto(Guid PlayerId,string PlayerName, int Lvl, string Region, string GameMode);
public record LeftMatchMakingDto(Guid PlayerId, string PlayerName, string Region, string GameMode);
public record RegionCreatedDto(string Name);
public record GamemodeCreatedDto(string Name,int MaxPlayers, int MaxScore);
public record GameCreated(string GameMode,int MaxScore, string Region, List<Player> Players);