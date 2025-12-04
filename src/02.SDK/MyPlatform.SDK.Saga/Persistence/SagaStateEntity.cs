namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// Entity for storing saga state in a database.
/// </summary>
public class SagaStateEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the saga instance.
    /// </summary>
    public string SagaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name/type of the saga.
    /// </summary>
    public string SagaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current state of the saga (Pending/Running/Completed/Failed/Compensating/Compensated).
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the saga data serialized as JSON.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets the current step index.
    /// </summary>
    public int CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the next retry time.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Gets or sets the step states serialized as JSON.
    /// </summary>
    public string? Steps { get; set; }
}
