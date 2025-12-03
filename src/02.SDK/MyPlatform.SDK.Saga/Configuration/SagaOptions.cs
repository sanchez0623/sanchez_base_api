namespace MyPlatform.SDK.Saga.Configuration;

/// <summary>
/// Configuration options for saga orchestration.
/// </summary>
public class SagaOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Saga";

    /// <summary>
    /// Gets or sets the state store type: InMemory, Database, or Redis.
    /// </summary>
    public string StateStore { get; set; } = "Database";

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

    /// <summary>
    /// Gets or sets the Redis-specific configuration options.
    /// </summary>
    public SagaRedisOptions? Redis { get; set; }

    /// <summary>
    /// Gets or sets the database-specific configuration options.
    /// </summary>
    public SagaDatabaseOptions? Database { get; set; }
}

/// <summary>
/// Redis-specific configuration options for saga state storage.
/// </summary>
public class SagaRedisOptions
{
    /// <summary>
    /// Gets or sets the key prefix for saga states in Redis.
    /// </summary>
    public string KeyPrefix { get; set; } = "saga:";

    /// <summary>
    /// Gets or sets the expiration time in minutes for saga states.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Database-specific configuration options for saga state storage.
/// </summary>
public class SagaDatabaseOptions
{
    /// <summary>
    /// Gets or sets the table name for saga states.
    /// </summary>
    public string TableName { get; set; } = "saga_states";

    /// <summary>
    /// Gets or sets a value indicating whether to automatically cleanup completed sagas.
    /// </summary>
    public bool AutoCleanupCompleted { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of days to retain completed sagas.
    /// </summary>
    public int RetentionDays { get; set; } = 7;
}
