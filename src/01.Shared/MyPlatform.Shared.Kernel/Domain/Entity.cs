namespace MyPlatform.Shared.Kernel.Domain;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public abstract class Entity<TKey> : IEquatable<Entity<TKey>> where TKey : notnull
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public virtual TKey Id { get; protected set; } = default!;

    /// <summary>
    /// Gets the creation time of this entity.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification time of this entity.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets or sets the last modifier identifier.
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TKey> entity && Equals(entity);
    }

    public bool Equals(Entity<TKey>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Sets the audit information for entity creation.
    /// </summary>
    /// <param name="userId">The user identifier who created this entity.</param>
    public virtual void SetCreatedAudit(string? userId = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    /// <summary>
    /// Sets the audit information for entity modification.
    /// </summary>
    /// <param name="userId">The user identifier who modified this entity.</param>
    public virtual void SetUpdatedAudit(string? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}

/// <summary>
/// Base class for entities with long identifier.
/// </summary>
public abstract class Entity : Entity<long>
{
}
