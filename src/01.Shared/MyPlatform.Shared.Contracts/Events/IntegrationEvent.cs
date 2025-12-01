namespace MyPlatform.Shared.Contracts.Events;

/// <summary>
/// Base class for integration events used for cross-service communication.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this event was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the correlation identifier for tracing across services.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the tenant identifier for multi-tenancy support.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the type name of this event.
    /// </summary>
    public string EventType => GetType().FullName ?? GetType().Name;
}
