using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.Configuration;
using MyPlatform.Shared.Contracts.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MyPlatform.SDK.EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of event publisher.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: _options.Durable,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", _options.HostName, _options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
            throw;
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized");
        }

        var eventName = typeof(TEvent).Name;
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // Persistent
        properties.MessageId = @event.EventId.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.ContentType = "application/json";
        properties.Type = @event.EventType;

        if (!string.IsNullOrEmpty(@event.CorrelationId))
        {
            properties.CorrelationId = @event.CorrelationId;
        }

        _channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: eventName,
            mandatory: true,
            basicProperties: properties,
            body: body);

        _logger.LogDebug("Published event {EventType} with ID {EventId}", eventName, @event.EventId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
