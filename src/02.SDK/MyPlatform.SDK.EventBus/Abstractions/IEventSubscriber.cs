using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.SDK.EventBus.Abstractions;

/// <summary>
/// Interface for subscribing to integration events.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to an integration event.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <typeparam name="THandler">The type of event handler.</typeparam>
    void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    /// <summary>
    /// Unsubscribes from an integration event.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <typeparam name="THandler">The type of event handler.</typeparam>
    void Unsubscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    /// <summary>
    /// Starts consuming messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}
