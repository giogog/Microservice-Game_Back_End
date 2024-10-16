using Match.Application;
using Match.Contracts;
using Match.Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Match.API.RabbitMQ;

public class MatchMakingEventHandler : BackgroundService
{
    private readonly string _queueName;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MatchMakingEventHandler> _logger;

    private IConnection _connection;
    private IModel _channel;

    // Services
    private readonly IEventProcessor _eventProcessor;
    private readonly IEventPublisher _eventPublisher;

    public MatchMakingEventHandler(IConfiguration configuration, ILogger<MatchMakingEventHandler> logger, IEventProcessor eventProcessor, IEventPublisher eventPublisher)
    {
        _configuration = configuration;
        _logger = logger;
        _eventProcessor = eventProcessor;
        _eventPublisher = eventPublisher;
        _queueName = configuration["Queues:MatchMaking"];
        InitializeRabbitMQ();
    }

    // Initialize RabbitMQ connection and declare exchange and queue
    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"])
        };

        try
        {
            // Establish connection and channel
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange and queue, ensuring durability
            _channel.ExchangeDeclare(exchange: "direct_trigger", type: ExchangeType.Direct);

            _channel.QueueDeclare(queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue to exchange with routing key
            _channel.QueueBind(queue: _queueName,
                exchange: "direct_trigger",
                routingKey: _queueName);

            // Attach event handler for connection shutdowns
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            _logger.LogInformation("RabbitMQ connection and channel initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not connect to the RabbitMQ: {ex.Message}");
        }
    }

    // Core execution loop for handling incoming messages from RabbitMQ
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        try
        {
            var consumer = new EventingBasicConsumer(_channel);

            // Event handler for message reception
            consumer.Received += (moduleHandle, ea) =>
            {
                _logger.LogInformation("Event received from RabbitMQ.");

                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                try
                {
                    // Process the event using the EventProcessor
                    var gameCreatedEvent = _eventProcessor.ProcessMatchMakingEvent(notificationMessage);

                    // If a new game is created, publish the event to the next queue
                    if (gameCreatedEvent != null)
                    {
                        string gameSessionQueue = _configuration["Queues:GameSession"];
                        _eventPublisher.SendMessage(gameCreatedEvent, gameSessionQueue);
                        _logger.LogInformation($"Game session created and event sent to {gameSessionQueue}.");
                    }

                    // Acknowledge message was successfully processed
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                    // Negative acknowledgement, requeue the message for later processing
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            // Start consuming messages from the queue
             _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            // Keep the service running indefinitely
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ExecuteAsync: {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Wait before retrying
        }
    }

    // Handler for handling RabbitMQ connection shutdown events
    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("RabbitMQ connection shutdown.");
    }

    // Dispose resources properly when the background service is stopped
    public override void Dispose()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }

        base.Dispose();
        _logger.LogInformation("RabbitMQ connection and channel disposed.");
    }
}
