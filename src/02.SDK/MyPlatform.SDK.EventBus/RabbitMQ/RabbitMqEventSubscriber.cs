using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.Configuration;
using MyPlatform.Shared.Contracts.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyPlatform.SDK.EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of event subscriber.
/// </summary>
public class RabbitMqEventSubscriber : IEventSubscriber, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, SubscriptionInfo> _subscriptions = new();
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;
    private bool _disposed;

    public RabbitMqEventSubscriber(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventSubscriber> logger,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private void InitializeConnection()
    {
        if (_connection is not null && _connection.IsOpen)
        {
            return;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: _options.Durable,
                autoDelete: false);

            // Declare queue
            _queueName = _options.QueueName ?? $"{_options.QueueNamePrefix}.messaging.{Environment.MachineName}";
            _channel.QueueDeclare(
                queue: _queueName,
                durable: _options.Durable,
                exclusive: false,
                autoDelete: false);

            // Set prefetch count for fair dispatch
            _channel.BasicQos(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false);

            _logger.LogInformation(
                "RabbitMQ subscriber connected to {Host}:{Port}, Queue: {Queue}",
                _options.HostName, _options.Port, _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
            throw;
        }
    }

    /// <inheritdoc />
    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (!_subscriptions.ContainsKey(eventName))
        {
            _subscriptions[eventName] = new SubscriptionInfo(typeof(TEvent), handlerType);
            _logger.LogInformation("Subscribed to event {EventName} with handler {HandlerType}", eventName, handlerType.Name);
        }
        else
        {
            _logger.LogWarning("Event {EventName} is already subscribed", eventName);
        }
    }

    /// <inheritdoc />
    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;

        if (_subscriptions.Remove(eventName))
        {
            _logger.LogInformation("Unsubscribed from event {EventName}", eventName);

            if (_channel is not null && _queueName is not null)
            {
                _channel.QueueUnbind(_queueName, _options.ExchangeName, eventName);
            }
        }
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        InitializeConnection();

        if (_channel is null || _queueName is null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized");
        }

        // Bind queue to exchange for each subscribed event
        foreach (var eventName in _subscriptions.Keys)
        {
            _channel.QueueBind(
                queue: _queueName,
                exchange: _options.ExchangeName,
                routingKey: eventName);

            _logger.LogDebug("Bound queue {Queue} to exchange {Exchange} with routing key {RoutingKey}",
                _queueName, _options.ExchangeName, eventName);
        }

        // Set up async consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceivedAsync;

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming messages from queue {Queue}", _queueName);

        await Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var eventName = ea.RoutingKey;
        var messageId = ea.BasicProperties.MessageId ?? "unknown";
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());

        _logger.LogDebug("Received message {MessageId} with routing key {RoutingKey}", messageId, eventName);

        try
        {
            if (_subscriptions.TryGetValue(eventName, out var subscriptionInfo))
            {
                await ProcessEventAsync(body, subscriptionInfo);
                _channel?.BasicAck(ea.DeliveryTag, multiple: false);
                _logger.LogDebug("Successfully processed message {MessageId}", messageId);
            }
            else
            {
                _logger.LogWarning("No handler found for event {EventName}, rejecting message", eventName);
                _channel?.BasicReject(ea.DeliveryTag, requeue: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} for event {EventName}", messageId, eventName);

            // Negative acknowledgment with requeue based on retry policy
            var shouldRequeue = ea.Redelivered == false;
            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: shouldRequeue);
        }
    }

    private async Task ProcessEventAsync(string message, SubscriptionInfo subscriptionInfo)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService(subscriptionInfo.HandlerType);

        var @event = JsonConvert.DeserializeObject(message, subscriptionInfo.EventType);
        if (@event is null)
        {
            throw new InvalidOperationException($"Failed to deserialize event of type {subscriptionInfo.EventType.Name}");
        }

        var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(subscriptionInfo.EventType);
        var handleMethod = handlerInterface.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync));

        if (handleMethod is null)
        {
            throw new InvalidOperationException($"HandleAsync method not found on handler {subscriptionInfo.HandlerType.Name}");
        }

        await (Task)handleMethod.Invoke(handler, new[] { @event, CancellationToken.None })!;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping RabbitMQ subscriber");

        _channel?.Close();
        _connection?.Close();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private record SubscriptionInfo(Type EventType, Type HandlerType);
}
