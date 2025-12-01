namespace MyPlatform.Shared.Contracts.Dtos;

/// <summary>
/// Base class for Data Transfer Objects.
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Base class for auditable DTOs.
/// </summary>
public abstract class AuditableDto : BaseDto
{
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modifier identifier.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
