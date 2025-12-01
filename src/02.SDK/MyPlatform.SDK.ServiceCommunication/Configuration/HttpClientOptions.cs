namespace MyPlatform.SDK.ServiceCommunication.Configuration;

/// <summary>
/// HTTP client configuration options.
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// Gets or sets the timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry sleep duration in milliseconds.
    /// </summary>
    public int RetrySleepDurationMs { get; set; } = 500;

    /// <summary>
    /// Gets or sets the circuit breaker duration in seconds.
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of exceptions before opening circuit breaker.
    /// </summary>
    public int CircuitBreakerExceptionsAllowed { get; set; } = 5;
}

/// <summary>
/// Service endpoint configuration.
/// </summary>
public class ServiceEndpoint
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key if required.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets custom headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];
}
