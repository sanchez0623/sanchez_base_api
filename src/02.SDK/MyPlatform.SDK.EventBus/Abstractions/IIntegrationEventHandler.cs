using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.SDK.EventBus.Abstractions;

/// <summary>
/// Handler for integration events.
/// </summary>
/// <typeparam name="TEvent">The type of event.</typeparam>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    /// <summary>
    /// Handles the integration event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
