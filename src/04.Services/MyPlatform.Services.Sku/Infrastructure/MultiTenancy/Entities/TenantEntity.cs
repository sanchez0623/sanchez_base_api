namespace MyPlatform.Services.Sku.Infrastructure.MultiTenancy.Entities;

/// <summary>
/// Entity representing a tenant stored in the database.
/// </summary>
/// <remarks>
/// This is an example implementation. Customize this entity based on your
/// specific business requirements.
/// </remarks>
public class TenantEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data isolation mode.
    /// Values: "Shared", "Isolated"
    /// </summary>
    public string IsolationMode { get; set; } = "Shared";

    /// <summary>
    /// Gets or sets the connection string for isolated tenants.
    /// Only applicable when IsolationMode is "Isolated".
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the tenant status.
    /// Values: "Active", "Suspended", "Deleted"
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Gets or sets the tenant configuration as JSON.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp (for soft delete).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
