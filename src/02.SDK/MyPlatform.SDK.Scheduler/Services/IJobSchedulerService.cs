using Quartz;

namespace MyPlatform.SDK.Scheduler.Services;

/// <summary>
/// Service for managing scheduled jobs.
/// </summary>
public interface IJobSchedulerService
{
    /// <summary>
    /// Schedules a job.
    /// </summary>
    /// <typeparam name="TJob">The type of job.</typeparam>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cronExpression">The cron expression.</param>
    /// <param name="data">Optional job data.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    Task ScheduleAsync<TJob>(string jobKey, string cronExpression, IDictionary<string, object>? data = null, string? tenantId = null)
        where TJob : IJob;

    /// <summary>
    /// Schedules a one-time job.
    /// </summary>
    /// <typeparam name="TJob">The type of job.</typeparam>
    /// <param name="jobKey">The job key.</param>
    /// <param name="runAt">When to run the job.</param>
    /// <param name="data">Optional job data.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    Task ScheduleOnceAsync<TJob>(string jobKey, DateTimeOffset runAt, IDictionary<string, object>? data = null, string? tenantId = null)
        where TJob : IJob;

    /// <summary>
    /// Pauses a job.
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    Task PauseAsync(string jobKey);

    /// <summary>
    /// Resumes a job.
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    Task ResumeAsync(string jobKey);

    /// <summary>
    /// Deletes a job.
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    Task DeleteAsync(string jobKey);

    /// <summary>
    /// Triggers a job immediately.
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    Task TriggerAsync(string jobKey);

    /// <summary>
    /// Checks if a job exists.
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    Task<bool> ExistsAsync(string jobKey);
}

/// <summary>
/// Quartz implementation of job scheduler service.
/// </summary>
public class QuartzJobSchedulerService : IJobSchedulerService
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzJobSchedulerService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    private async Task<IScheduler> GetSchedulerAsync()
    {
        return await _schedulerFactory.GetScheduler();
    }

    /// <inheritdoc />
    public async Task ScheduleAsync<TJob>(string jobKey, string cronExpression, IDictionary<string, object>? data = null, string? tenantId = null)
        where TJob : IJob
    {
        var scheduler = await GetSchedulerAsync();
        var key = new JobKey(jobKey);

        if (await scheduler.CheckExists(key))
        {
            await scheduler.DeleteJob(key);
        }

        var jobBuilder = JobBuilder.Create<TJob>()
            .WithIdentity(key)
            .StoreDurably();

        if (data is not null)
        {
            jobBuilder.SetJobData(new JobDataMap(data));
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            jobBuilder.UsingJobData("TenantId", tenantId);
        }

        var job = jobBuilder.Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey}-trigger")
            .WithCronSchedule(cronExpression)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    /// <inheritdoc />
    public async Task ScheduleOnceAsync<TJob>(string jobKey, DateTimeOffset runAt, IDictionary<string, object>? data = null, string? tenantId = null)
        where TJob : IJob
    {
        var scheduler = await GetSchedulerAsync();
        var key = new JobKey(jobKey);

        if (await scheduler.CheckExists(key))
        {
            await scheduler.DeleteJob(key);
        }

        var jobBuilder = JobBuilder.Create<TJob>()
            .WithIdentity(key)
            .StoreDurably();

        if (data is not null)
        {
            jobBuilder.SetJobData(new JobDataMap(data));
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            jobBuilder.UsingJobData("TenantId", tenantId);
        }

        var job = jobBuilder.Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey}-trigger")
            .StartAt(runAt)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    /// <inheritdoc />
    public async Task PauseAsync(string jobKey)
    {
        var scheduler = await GetSchedulerAsync();
        await scheduler.PauseJob(new JobKey(jobKey));
    }

    /// <inheritdoc />
    public async Task ResumeAsync(string jobKey)
    {
        var scheduler = await GetSchedulerAsync();
        await scheduler.ResumeJob(new JobKey(jobKey));
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string jobKey)
    {
        var scheduler = await GetSchedulerAsync();
        await scheduler.DeleteJob(new JobKey(jobKey));
    }

    /// <inheritdoc />
    public async Task TriggerAsync(string jobKey)
    {
        var scheduler = await GetSchedulerAsync();
        await scheduler.TriggerJob(new JobKey(jobKey));
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string jobKey)
    {
        var scheduler = await GetSchedulerAsync();
        return await scheduler.CheckExists(new JobKey(jobKey));
    }
}
