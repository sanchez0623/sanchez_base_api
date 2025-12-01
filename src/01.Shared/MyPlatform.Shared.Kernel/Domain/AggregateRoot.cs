using MyPlatform.Shared.Kernel.Events;

namespace MyPlatform.Shared.Kernel.Domain;

/// <summary>
/// Base class for aggregate roots that support domain events.
/// </summary>
/// <typeparam name="TKey">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets or sets the concurrency token for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; protected set; }

    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenancy support.
    /// </summary>
    public string? TenantId { get; protected set; }

    /// <summary>
    /// Adds a domain event to be raised when the aggregate is saved.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from the collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the collection.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Sets the tenant identifier for this aggregate.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    public virtual void SetTenant(string tenantId)
    {
        TenantId = tenantId;
    }
}

/// <summary>
/// Base class for aggregate roots with long identifier.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<long>
{
}
