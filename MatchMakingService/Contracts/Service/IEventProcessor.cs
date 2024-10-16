using Match.Domain.Models;

namespace Match.Contracts;

public interface IEventProcessor
{
    GameCreated? ProcessMatchMakingEvent(string @event);

}
