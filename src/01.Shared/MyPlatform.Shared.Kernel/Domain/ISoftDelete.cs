namespace MyPlatform.Shared.Kernel.Domain;

/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Gets or sets the deletion time.
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity.
    /// </summary>
    string? DeletedBy { get; }

    /// <summary>
    /// Marks this entity as deleted.
    /// </summary>
    /// <param name="deletedBy">The identifier of the user who deleted this entity.</param>
    void MarkAsDeleted(string? deletedBy = null);

    /// <summary>
    /// Restores this entity from deleted state.
    /// </summary>
    void Restore();
}

/// <summary>
/// Base implementation for soft-deletable entities.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public abstract class SoftDeleteEntity<TKey> : Entity<TKey>, ISoftDelete where TKey : notnull
{
    /// <inheritdoc />
    public bool IsDeleted { get; protected set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; protected set; }

    /// <inheritdoc />
    public string? DeletedBy { get; protected set; }

    /// <inheritdoc />
    public void MarkAsDeleted(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <inheritdoc />
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
