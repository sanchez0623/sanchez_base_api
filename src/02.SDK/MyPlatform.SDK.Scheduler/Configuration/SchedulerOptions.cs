namespace MyPlatform.SDK.Scheduler.Configuration;

/// <summary>
/// Scheduler configuration options.
/// </summary>
public class SchedulerOptions
{
    /// <summary>
    /// Gets or sets the scheduler instance name.
    /// </summary>
    public string InstanceName { get; set; } = "MyPlatformScheduler";

    /// <summary>
    /// Gets or sets a value indicating whether to use persistent store.
    /// </summary>
    public bool UsePersistentStore { get; set; }

    /// <summary>
    /// Gets or sets the database connection string for persistent store.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the thread count for the scheduler.
    /// </summary>
    public int ThreadCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to wait for jobs to complete on shutdown.
    /// </summary>
    public bool WaitForJobsToComplete { get; set; } = true;

    /// <summary>
    /// Gets or sets the misfire threshold in seconds.
    /// </summary>
    public int MisfireThresholdSeconds { get; set; } = 60;
}
