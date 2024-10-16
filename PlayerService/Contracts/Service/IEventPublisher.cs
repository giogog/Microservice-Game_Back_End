using Domain.Models;

namespace Contracts;

public interface IEventPublisher
{
    void SendMessage<T>(T message, string queueName);
}