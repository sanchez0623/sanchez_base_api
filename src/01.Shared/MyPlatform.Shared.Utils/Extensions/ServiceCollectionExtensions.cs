using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Shared.Utils.Generators;

namespace MyPlatform.Shared.Utils.Extensions;

/// <summary>
/// Extension methods for registering Utils services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Shared Utils services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure SnowflakeId options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedUtils(this IServiceCollection services, Action<SnowflakeIdOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<SnowflakeIdOptions>(options =>
            {
                options.WorkerId = 1;
                options.DatacenterId = 1;
            });
        }

        services.AddSingleton<SnowflakeIdGenerator>();
        return services;
    }
}
