namespace MyPlatform.SDK.Idempotency.Configuration;

/// <summary>
/// Idempotency configuration options.
/// </summary>
public class IdempotencyOptions
{
    /// <summary>
    /// Gets or sets the header name for the idempotency key.
    /// </summary>
    public string HeaderName { get; set; } = "X-Idempotency-Key";

    /// <summary>
    /// Gets or sets the default expiration time for idempotency keys in seconds.
    /// </summary>
    public int DefaultExpirationSeconds { get; set; } = 86400; // 24 hours

    /// <summary>
    /// Gets or sets the lock timeout in seconds.
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum wait time for acquiring a lock in seconds.
    /// </summary>
    public int LockWaitTimeSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to return cached results for duplicate requests.
    /// </summary>
    public bool EnableResultCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to require idempotency key for all POST/PUT/PATCH requests.
    /// </summary>
    public bool RequireIdempotencyKey { get; set; }
}
