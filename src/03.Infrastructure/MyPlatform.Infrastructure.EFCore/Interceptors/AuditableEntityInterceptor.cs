using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Infrastructure.EFCore.Interceptors;

/// <summary>
/// Interceptor for automatically setting audit fields on entities.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly Func<string?>? _getCurrentUserId;

    public AuditableEntityInterceptor(Func<string?>? getCurrentUserId = null)
    {
        _getCurrentUserId = getCurrentUserId;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var userId = _getCurrentUserId?.Invoke();
        var now = DateTime.UtcNow;

        foreach (var entry in eventData.Context.ChangeTracker.Entries<Entity<long>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedAudit(userId);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedAudit(userId);
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
