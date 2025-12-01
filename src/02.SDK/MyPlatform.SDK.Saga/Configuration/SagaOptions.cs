namespace MyPlatform.SDK.Saga.Configuration;

/// <summary>
/// Configuration options for saga orchestration.
/// </summary>
public class SagaOptions
{
    /// <summary>
    /// Gets or sets the default timeout for saga execution in seconds.
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the default retry count for failed steps.
    /// </summary>
    public int DefaultRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay in seconds.
    /// </summary>
    public int InitialRetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the retry delay multiplier for exponential backoff.
    /// </summary>
    public double RetryDelayMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the maximum retry delay in seconds.
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-cleanup completed sagas.
    /// </summary>
    public bool AutoCleanupCompleted { get; set; }

    /// <summary>
    /// Gets or sets the number of days to keep completed sagas before cleanup.
    /// </summary>
    public int CompletedRetentionDays { get; set; } = 7;
}
