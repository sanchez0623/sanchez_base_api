using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Infrastructure.Redis.Services;
using MyPlatform.SDK.Idempotency.Configuration;
using MyPlatform.SDK.Idempotency.Filters;
using MyPlatform.SDK.Idempotency.Services;

namespace MyPlatform.SDK.Idempotency.Extensions;

/// <summary>
/// Extension methods for registering Idempotency services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds idempotency services to the service collection.
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
}
