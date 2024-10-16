using Domain.Models;

namespace Contracts;

public interface IGamemodeService
{
    IEnumerable<GamemodeDto>? GetGamemodes();
}
