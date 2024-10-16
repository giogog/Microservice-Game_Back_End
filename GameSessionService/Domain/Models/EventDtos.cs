namespace GameSession.Domain.Models;

public record GameCreatedEventDto(string GameMode, string Region, Guid[] Players);
