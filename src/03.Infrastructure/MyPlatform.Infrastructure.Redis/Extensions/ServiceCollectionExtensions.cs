using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Infrastructure.Redis.Configuration;
using MyPlatform.Infrastructure.Redis.Services;
using StackExchange.Redis;

namespace MyPlatform.Infrastructure.Redis.Extensions;

/// <summary>
/// Extension methods for registering Redis services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSection = configuration.GetSection(RedisOptions.SectionName);
        services.Configure<RedisOptions>(redisSection);

        var options = redisSection.Get<RedisOptions>() ?? new RedisOptions();

        var configurationOptions = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            ConnectTimeout = options.ConnectTimeoutMs,
            SyncTimeout = options.SyncTimeoutMs,
            Ssl = options.UseSsl,
            DefaultDatabase = options.Database
        };

        foreach (var endpoint in options.ConnectionString.Split(','))
        {
            configurationOptions.EndPoints.Add(endpoint.Trim());
        }

        if (!string.IsNullOrEmpty(options.Password))
        {
            configurationOptions.Password = options.Password;
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configurationOptions));

        services.AddSingleton<IRedisCacheService>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisCacheService(redis, options.InstanceName);
        });

        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        return services;
    }
}
