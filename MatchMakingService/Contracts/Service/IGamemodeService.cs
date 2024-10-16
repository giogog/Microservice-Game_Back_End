using Match.Domain.Models;

namespace Match.Contracts;

public interface IGamemodeService
{
    GamemodeDto? GetGamemode(string name);
    IEnumerable<GamemodeDto>? GetGamemodes();
}
