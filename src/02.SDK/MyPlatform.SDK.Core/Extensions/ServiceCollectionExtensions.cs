using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Core.Configuration;
using MyPlatform.Shared.Kernel.Extensions;
using MyPlatform.Shared.Utils.Extensions;

namespace MyPlatform.SDK.Core.Extensions;

/// <summary>
/// Extension methods for registering SDK Core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core platform services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PlatformOptions>(configuration.GetSection("Platform"));

        services.AddSharedKernel();
        services.AddSharedUtils();

        return services;
    }

    /// <summary>
    /// Adds the core platform services to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure platform options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformCore(this IServiceCollection services, Action<PlatformOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSharedKernel();
        services.AddSharedUtils();

        return services;
    }
}
