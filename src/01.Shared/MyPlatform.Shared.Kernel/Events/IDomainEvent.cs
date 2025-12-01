namespace MyPlatform.Shared.Kernel.Events;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the type name of this event.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base implementation of domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <inheritdoc />
    public string EventType => GetType().Name;
}
