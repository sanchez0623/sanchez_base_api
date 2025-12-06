using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.Configuration;
using MyPlatform.SDK.EventBus.Outbox;
using MyPlatform.SDK.EventBus.RabbitMQ;
using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.SDK.EventBus.Extensions;

/// <summary>
/// Extension methods for registering EventBus services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EventBus services with RabbitMQ to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();

        return services;
    }

    /// <summary>
    /// Adds EventBus subscriber services with RabbitMQ to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>An EventBusBuilder for registering event handlers.</returns>
    public static EventBusBuilder AddPlatformEventBusSubscriber(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventSubscriber, RabbitMqEventSubscriber>();
        services.AddHostedService<EventBusConsumerHostedService>();

        return new EventBusBuilder(services);
    }

    /// <summary>
    /// Adds a custom outbox store implementation.
    /// </summary>
    /// <typeparam name="T">The type of outbox store.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOutboxStore<T>(this IServiceCollection services)
        where T : class, IOutboxStore
    {
        services.AddSingleton<IOutboxStore, T>();
        return services;
    }
}

/// <summary>
/// Builder for configuring EventBus subscriptions.
/// </summary>
public class EventBusBuilder
{
    private readonly IServiceCollection _services;
    internal List<Action<IEventSubscriber>> SubscriptionActions { get; } = new();

    public EventBusBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Adds an event handler for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <typeparam name="THandler">The type of event handler.</typeparam>
    /// <returns>The builder for chaining.</returns>
    public EventBusBuilder AddEventHandler<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        _services.AddScoped<THandler>();
        _services.AddScoped(typeof(IIntegrationEventHandler<TEvent>), typeof(THandler));
        SubscriptionActions.Add(subscriber => subscriber.Subscribe<TEvent, THandler>());
        return this;
    }

    /// <summary>
    /// Builds the subscriptions by registering a subscription configuration.
    /// </summary>
    public IServiceCollection Build()
    {
        _services.AddSingleton(this);
        return _services;
    }
}

/// <summary>
/// Hosted service that manages event bus consumer lifecycle.
/// </summary>
public class EventBusConsumerHostedService : IHostedService
{
    private readonly IEventSubscriber _subscriber;
    private readonly EventBusBuilder _builder;
    private readonly ILogger<EventBusConsumerHostedService> _logger;

    public EventBusConsumerHostedService(
        IEventSubscriber subscriber,
        EventBusBuilder builder,
        ILogger<EventBusConsumerHostedService> logger)
    {
        _subscriber = subscriber;
        _builder = builder;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting EventBus consumer...");

        // Apply all subscriptions
        foreach (var action in _builder.SubscriptionActions)
        {
            action(_subscriber);
        }

        await _subscriber.StartAsync(cancellationToken);

        _logger.LogInformation("EventBus consumer started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping EventBus consumer...");
        await _subscriber.StopAsync(cancellationToken);
        _logger.LogInformation("EventBus consumer stopped");
    }
}
