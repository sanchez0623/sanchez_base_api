using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Infrastructure.EFCore.Sharding;

namespace MyPlatform.Infrastructure.EFCore.Extensions;

/// <summary>
/// Extension methods for configuring database sharding functionality.
/// </summary>
public static class ShardingExtensions
{
    /// <summary>
    /// Adds sharding support to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharding<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        services.Configure<ShardingOptions>(
            configuration.GetSection(ShardingOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds sharding support with custom options configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharding<TContext>(
        this IServiceCollection services,
        Action<ShardingOptions> configureOptions)
        where TContext : DbContext
    {
        services.Configure(configureOptions);

        return services;
    }

    /// <summary>
    /// Registers a sharding route rule for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRoute">The route rule type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddShardingRoute<TEntity, TRoute>(this IServiceCollection services)
        where TEntity : class
        where TRoute : class, IShardingRouteRule<TEntity>
    {
        services.AddSingleton<IShardingRouteRule<TEntity>, TRoute>();

        return services;
    }

    /// <summary>
    /// Registers a DateTime-based sharding route for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDateTimeShardingRoute<TEntity>(this IServiceCollection services)
        where TEntity : class
    {
        return services.AddShardingRoute<TEntity, DateTimeShardingRoute<TEntity>>();
    }

    /// <summary>
    /// Registers a modulo-based sharding route for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModShardingRoute<TEntity>(this IServiceCollection services)
        where TEntity : class
    {
        return services.AddShardingRoute<TEntity, ModShardingRoute<TEntity>>();
    }

    /// <summary>
    /// Registers a tenant-based sharding route for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantShardingRoute<TEntity>(this IServiceCollection services)
        where TEntity : class
    {
        return services.AddShardingRoute<TEntity, TenantShardingRoute<TEntity>>();
    }
}
