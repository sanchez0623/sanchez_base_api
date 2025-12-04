namespace MyPlatform.SDK.MultiTenancy.Models;

/// <summary>
/// Represents complete tenant information including connection configuration.
/// </summary>
public class TenantInfo
{
    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data isolation mode for this tenant.
    /// </summary>
    public TenantIsolationMode IsolationMode { get; set; } = TenantIsolationMode.Shared;

    /// <summary>
    /// Gets or sets the connection string for this tenant.
    /// Only applicable when IsolationMode is Isolated.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the status of this tenant.
    /// </summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// Gets or sets additional configuration key-value pairs for this tenant.
    /// </summary>
    public Dictionary<string, string> Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets the date and time when this tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this tenant was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
