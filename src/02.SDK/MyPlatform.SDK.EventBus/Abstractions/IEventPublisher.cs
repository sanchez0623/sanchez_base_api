using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.SDK.EventBus.Abstractions;

/// <summary>
/// Interface for publishing integration events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an integration event.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// Publishes multiple integration events.
    /// </summary>
    /// <typeparam name="TEvent">The type of events.</typeparam>
    /// <param name="events">The events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
