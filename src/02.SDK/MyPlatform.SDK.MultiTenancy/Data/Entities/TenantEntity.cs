using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPlatform.SDK.MultiTenancy.Data.Entities;

/// <summary>
/// Represents a tenant record in the database.
/// </summary>
[Table("tenants")]
public class TenantEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("tenant_id")]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data isolation mode for this tenant.
    /// Values: Shared, Isolated
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("isolation_mode")]
    public string IsolationMode { get; set; } = "Shared";

    /// <summary>
    /// Gets or sets the connection string for this tenant.
    /// Only applicable when IsolationMode is Isolated.
    /// </summary>
    [MaxLength(500)]
    [Column("connection_string")]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the status of this tenant.
    /// Values: Active, Suspended, Deleted
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Gets or sets the additional configuration for this tenant as JSON.
    /// </summary>
    [Column("configuration", TypeName = "json")]
    public string? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this tenant was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this tenant was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this tenant was deleted (soft delete).
    /// </summary>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
