namespace MyPlatform.SDK.EventBus.Configuration;

/// <summary>
/// RabbitMQ configuration options.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string ExchangeName { get; set; } = "myplatform.events";

    /// <summary>
    /// Gets or sets the exchange type.
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Gets or sets the queue name prefix.
    /// </summary>
    public string QueueNamePrefix { get; set; } = "myplatform";

    /// <summary>
    /// Gets or sets the specific queue name. If null, a name will be auto-generated.
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use durable exchanges and queues.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets the retry count for failed messages.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry delay in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the prefetch count.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}
