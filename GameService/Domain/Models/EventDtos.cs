namespace Game.Domain.Models;


public record RegionCreated(string Name,string Event);

public record GamemodeCreated(string Name, int MaxPlayers, int MaxScore, string Event);

