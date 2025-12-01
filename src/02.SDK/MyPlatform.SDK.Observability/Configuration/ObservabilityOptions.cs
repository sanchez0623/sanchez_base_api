namespace MyPlatform.SDK.Observability.Configuration;

/// <summary>
/// Observability configuration options.
/// </summary>
public class ObservabilityOptions
{
    /// <summary>
    /// Gets or sets the service name for tracing.
    /// </summary>
    public string ServiceName { get; set; } = "MyPlatform.Service";

    /// <summary>
    /// Gets or sets the service version.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Gets or sets a value indicating whether to enable console exporter.
    /// </summary>
    public bool EnableConsoleExporter { get; set; }

    /// <summary>
    /// Gets or sets the OTLP exporter endpoint.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets logging configuration.
    /// </summary>
    public LoggingOptions Logging { get; set; } = new();

    /// <summary>
    /// Gets or sets tracing configuration.
    /// </summary>
    public TracingOptions Tracing { get; set; } = new();

    /// <summary>
    /// Gets or sets metrics configuration.
    /// </summary>
    public MetricsOptions Metrics { get; set; } = new();
}

/// <summary>
/// Logging configuration options.
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Gets or sets a value indicating whether to write to console.
    /// </summary>
    public bool WriteToConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to write to file.
    /// </summary>
    public bool WriteToFile { get; set; }

    /// <summary>
    /// Gets or sets the log file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the file rolling interval.
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// Gets or sets the file size limit in bytes.
    /// </summary>
    public long? FileSizeLimitBytes { get; set; }

    /// <summary>
    /// Gets or sets the retained file count limit.
    /// </summary>
    public int? RetainedFileCountLimit { get; set; }
}

/// <summary>
/// Tracing configuration options.
/// </summary>
public class TracingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether tracing is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trace HTTP client requests.
    /// </summary>
    public bool TraceHttpClient { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trace ASP.NET Core requests.
    /// </summary>
    public bool TraceAspNetCore { get; set; } = true;

    /// <summary>
    /// Gets or sets the sampling ratio (0.0 - 1.0).
    /// </summary>
    public double SamplingRatio { get; set; } = 1.0;
}

/// <summary>
/// Metrics configuration options.
/// </summary>
public class MetricsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether metrics are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to expose Prometheus endpoint.
    /// </summary>
    public bool ExposePrometheus { get; set; } = true;

    /// <summary>
    /// Gets or sets the Prometheus endpoint path.
    /// </summary>
    public string PrometheusPath { get; set; } = "/metrics";
}
