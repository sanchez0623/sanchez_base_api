using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Caching.Configuration;
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
        services.Configure<CacheOptions>(configuration.GetSection("Cache"));
        services.AddMemoryCache();
        services.AddScoped<IMultiLevelCache, MultiLevelCache>();

        return services;
    }
}
