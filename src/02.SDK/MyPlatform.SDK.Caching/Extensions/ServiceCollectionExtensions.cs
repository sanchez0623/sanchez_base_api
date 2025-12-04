using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Caching.Configuration;
using MyPlatform.SDK.Caching.Invalidation;
using MyPlatform.SDK.Caching.Services;

namespace MyPlatform.SDK.Caching.Extensions;

/// <summary>
/// Extension methods for registering Caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-level caching services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheSection = configuration.GetSection("Cache");
        services.Configure<CacheOptions>(cacheSection);
        services.AddMemoryCache();

        // Read configuration at startup time to determine if invalidation notification is enabled
        // Note: This is a startup-time decision; changes to EnableInvalidationNotification
        // require an application restart to take effect
        var options = cacheSection.Get<CacheOptions>() ?? new CacheOptions();

        // Register cache invalidation services if enabled
        if (options.EnableInvalidationNotification)
        {
            services.AddSingleton<ICacheInvalidationNotifier, RedisCacheInvalidationNotifier>();
            services.AddHostedService<CacheInvalidationSubscriber>();
        }

        services.AddScoped<IMultiLevelCache, MultiLevelCache>();

        return services;
    }
}
