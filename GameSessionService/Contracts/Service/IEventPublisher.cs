namespace GameSession.Contracts;

public interface IEventPublisher
{
    void SendMessage<T>(T message, string routingKey);

}
