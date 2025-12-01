using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Infrastructure.EFCore.Interceptors;

/// <summary>
/// Interceptor for automatically applying soft delete behavior.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Change delete to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.MarkAsDeleted();
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
