using Match.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Match.Infrastructure.Services;


public class EventPublisher : IEventPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventPublisher> _logger;

    private readonly List<string> _declaredQueues = new List<string>();

    private IConnection _connection;
    private IModel _channel;

    public EventPublisher(IConfiguration configuration, ILogger<EventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;



        InitializeRabbitMQ();
    }

    public void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"])
        };

        try
        {
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "direct_trigger", type: ExchangeType.Direct);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            _logger.LogInformation("Connected to MessageBus");

        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not connect to the Message Bus: {ex.Message}");
        }

    }
    public void SendMessage<T>(T message, string routingKey)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        if (!_declaredQueues.Contains(routingKey))
        {
            _channel.QueueDeclare(queue: routingKey,
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
            _channel.QueueBind(queue: routingKey, exchange: "direct_trigger", routingKey: routingKey);
            _declaredQueues.Add(routingKey);
        }

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;


        try
        {
            _channel.BasicPublish(exchange: "direct_trigger",
                routingKey: routingKey,
                basicProperties: properties,
                body: body);
            _logger.LogInformation($"Sent message to routing key '{routingKey}': {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Sending event to queue failed: {ex.Message}");
        }

    }

    public void Dispose()
    {
        _logger.LogInformation("MessageBus Disposed");
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
        }
        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
        }
    }


    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        _logger.LogInformation("RabbitMQ Connection Shutdown");
    }
}
