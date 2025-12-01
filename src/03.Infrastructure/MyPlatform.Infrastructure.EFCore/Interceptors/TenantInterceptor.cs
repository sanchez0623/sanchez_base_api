using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Infrastructure.EFCore.Interceptors;

/// <summary>
/// Interceptor for automatically setting tenant ID on entities.
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || !_tenantContext.HasTenant)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in eventData.Context.ChangeTracker.Entries<AggregateRoot<long>>())
        {
            if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.TenantId))
            {
                entry.Entity.SetTenant(_tenantContext.TenantId!);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
