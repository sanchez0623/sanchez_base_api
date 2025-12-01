using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Scheduler.Configuration;
using MyPlatform.SDK.Scheduler.Services;
using Quartz;

namespace MyPlatform.SDK.Scheduler.Extensions;

/// <summary>
/// Extension methods for registering Scheduler services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Quartz.NET scheduler services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureJobs">Optional action to configure jobs.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformScheduler(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollectionQuartzConfigurator>? configureJobs = null)
    {
        services.Configure<SchedulerOptions>(configuration.GetSection("Scheduler"));
        var options = configuration.GetSection("Scheduler").Get<SchedulerOptions>() ?? new SchedulerOptions();

        services.AddQuartz(q =>
        {
            q.SchedulerName = options.InstanceName;
            q.UseMicrosoftDependencyInjectionJobFactory();

            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = options.ThreadCount;
            });

            configureJobs?.Invoke(q);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddSingleton<IJobSchedulerService, QuartzJobSchedulerService>();

        return services;
    }
}
