namespace MyPlatform.SDK.Saga.Models;

/// <summary>
/// Represents the state of a saga instance.
/// </summary>
public class SagaState
{
    /// <summary>
    /// Gets or sets the unique identifier of the saga instance.
    /// </summary>
    public string SagaId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the saga.
    /// </summary>
    public string SagaName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the saga.
    /// </summary>
    public SagaStatus Status { get; set; } = SagaStatus.Pending;

    /// <summary>
    /// Gets or sets the current step index.
    /// </summary>
    public int CurrentStepIndex { get; set; }

    /// <summary>
    /// Gets or sets the serialized saga data.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets the step states.
    /// </summary>
    public List<SagaStepState> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the next retry time.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
}

/// <summary>
/// Represents the state of a saga step.
/// </summary>
public class SagaStepState
{
    /// <summary>
    /// Gets or sets the step index.
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    /// Gets or sets the step name.
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step status.
    /// </summary>
    public SagaStepStatus Status { get; set; } = SagaStepStatus.Pending;

    /// <summary>
    /// Gets or sets the step result data.
    /// </summary>
    public string? ResultData { get; set; }

    /// <summary>
    /// Gets or sets the error message if the step failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the start timestamp.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the retry count for this step.
    /// </summary>
    public int RetryCount { get; set; }
}
