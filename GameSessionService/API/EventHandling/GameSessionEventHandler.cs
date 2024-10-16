using GameSession.Application.MediatR.Commands;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GameSession.API.RabbitMQ;

public class GameSessionEventHandler : BackgroundService
{
    private readonly string _queueName;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GameSessionEventHandler> _logger;

    private IConnection _connection;
    private IModel _channel;

    // Services
    private readonly IMediator _mediator;

    public GameSessionEventHandler(IConfiguration configuration, ILogger<GameSessionEventHandler> logger, IMediator mediator)
    {
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
        _queueName = configuration["Queues:GameSession"];
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
            consumer.Received += async (moduleHandle, ea) =>
            {
                _logger.LogInformation("Event received from RabbitMQ.");

                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                try
                {

                    // Deserialize the message to CreateGameSessionCommand
                    var createGameSessionCommand = JsonSerializer.Deserialize<CreateGameSessionCommand>(notificationMessage);

                    if (createGameSessionCommand == null)
                    {
                        _logger.LogWarning("Received invalid or empty CreateGameSessionCommand.");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    // Acknowledge message was successfully received and processed
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    // Send the command via MediatR
                    await _mediator.Send(createGameSessionCommand);
                    _logger.LogInformation("CreateGameSessionCommand successfully processed.");
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError($"Invalid JSON format: {jsonEx.Message}");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false); // Do not requeue invalid JSON
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true); // Requeue on other errors
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
