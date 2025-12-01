namespace MyPlatform.Shared.Kernel.Domain;

/// <summary>
/// Marker interface for aggregate roots.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Clears all domain events from the aggregate.
    /// </summary>
    void ClearDomainEvents();
}
