using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Infrastructure.Redis.Services;
using MyPlatform.SDK.Idempotency.Configuration;
using MyPlatform.SDK.Idempotency.Filters;
using MyPlatform.SDK.Idempotency.Services;
using StackExchange.Redis;

namespace MyPlatform.SDK.Idempotency.Extensions;

/// <summary>
/// Extension methods for registering Idempotency services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP API idempotency services to the service collection.
    /// Used for ensuring HTTP requests are processed only once.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformIdempotency(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(configuration.GetSection("Idempotency"));

        var options = configuration.GetSection("Idempotency").Get<IdempotencyOptions>() ?? new IdempotencyOptions();

        services.AddScoped<IIdempotencyService>(sp =>
        {
            var cacheService = sp.GetRequiredService<IRedisCacheService>();
            var lockService = sp.GetRequiredService<IDistributedLockService>();

            return new RedisIdempotencyService(
                cacheService,
                lockService,
                TimeSpan.FromSeconds(options.DefaultExpirationSeconds),
                TimeSpan.FromSeconds(options.LockTimeoutSeconds),
                TimeSpan.FromSeconds(options.LockWaitTimeSeconds));
        });

        services.AddScoped<IdempotencyFilter>();

        return services;
    }

    /// <summary>
    /// Adds event consumer idempotency services to the service collection.
    /// Used for ensuring message queue events are processed only once.
    /// Supports 10W+ QPS with Redis-based atomic operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Requires Redis (IConnectionMultiplexer) to be registered.
    /// Use AddPlatformRedis() before calling this method.
    /// </remarks>
    public static IServiceCollection AddEventIdempotency(this IServiceCollection services)
    {
        services.AddSingleton<IEventIdempotencyChecker, RedisEventIdempotencyChecker>();
        return services;
    }
}
