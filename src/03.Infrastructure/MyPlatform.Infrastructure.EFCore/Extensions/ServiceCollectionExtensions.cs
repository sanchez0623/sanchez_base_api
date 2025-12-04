using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyPlatform.Infrastructure.EFCore.Interceptors;
using MyPlatform.Infrastructure.EFCore.ReadWriteSplit;
using MyPlatform.Infrastructure.EFCore.Repositories;
using MyPlatform.Infrastructure.EFCore.UnitOfWork;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Shared.Kernel.Domain;
using MyPlatform.Shared.Kernel.Repositories;

namespace MyPlatform.Infrastructure.EFCore.Extensions;

/// <summary>
/// Extension methods for registering EF Core infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core infrastructure services to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">Action to configure DbContext options.</param>
    /// <param name="getCurrentUserId">Optional function to get current user ID for auditing.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformEfCore<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
        Func<IServiceProvider, string?>? getCurrentUserId = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((sp, options) =>
        {
            optionsAction(sp, options);

            // Add read-write split interceptor if enabled
            var readWriteOptions = sp.GetService<IOptions<ReadWriteOptions>>()?.Value;
            if (readWriteOptions?.Enabled == true)
            {
                var resolver = sp.GetRequiredService<IConnectionStringResolver>();
                options.AddInterceptors(new ReadWriteDbCommandInterceptor(resolver));
            }

            // Add interceptors
            options.AddInterceptors(new AuditableEntityInterceptor(() => getCurrentUserId?.Invoke(sp)));
            options.AddInterceptors(new SoftDeleteInterceptor());

            var tenantContext = sp.GetService<ITenantContext>();
            if (tenantContext is not null)
            {
                options.AddInterceptors(new TenantInterceptor(tenantContext));
            }
        });

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();

        return services;
    }

    /// <summary>
    /// Registers a repository for an aggregate root.
    /// </summary>
    /// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepository<TAggregate, TContext>(this IServiceCollection services)
        where TAggregate : AggregateRoot<long>
        where TContext : DbContext
    {
        services.AddScoped<IRepository<TAggregate>, EfCoreRepository<TAggregate, TContext>>();
        return services;
    }

    /// <summary>
    /// Registers a custom repository implementation.
    /// </summary>
    /// <typeparam name="TInterface">The repository interface.</typeparam>
    /// <typeparam name="TImplementation">The repository implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomRepository<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TInterface, TImplementation>();
        return services;
    }
}
