namespace MyPlatform.SDK.Core.Configuration;

/// <summary>
/// Global platform configuration options.
/// </summary>
public class PlatformOptions
{
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string ApplicationName { get; set; } = "MyPlatform";

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    public string ServiceId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed error messages.
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Gets or sets the default page size for pagination.
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum page size for pagination.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
