using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.MultiTenancy.Services;
using Quartz;

namespace MyPlatform.SDK.Scheduler.Jobs;

/// <summary>
/// Base class for tenant-aware jobs.
/// </summary>
public abstract class TenantAwareJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    protected TenantAwareJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the tenant ID from the job context.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <returns>The tenant ID if available.</returns>
    protected string? GetTenantId(IJobExecutionContext context)
    {
        return context.MergedJobDataMap.GetString("TenantId");
    }

    /// <summary>
    /// Executes the job in the tenant context.
    /// </summary>
    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();

        var tenantId = GetTenantId(context);
        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }

        await ExecuteInTenantContextAsync(context, scope.ServiceProvider);
    }

    /// <summary>
    /// Executes the job logic within the tenant context.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="serviceProvider">The scoped service provider.</param>
    protected abstract Task ExecuteInTenantContextAsync(IJobExecutionContext context, IServiceProvider serviceProvider);
}
