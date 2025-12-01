using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.Configuration;
using MyPlatform.SDK.EventBus.Outbox;
using MyPlatform.SDK.EventBus.RabbitMQ;

namespace MyPlatform.SDK.EventBus.Extensions;

/// <summary>
/// Extension methods for registering EventBus services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EventBus services with RabbitMQ to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();

        return services;
    }

    /// <summary>
    /// Adds a custom outbox store implementation.
    /// </summary>
    /// <typeparam name="T">The type of outbox store.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOutboxStore<T>(this IServiceCollection services)
        where T : class, IOutboxStore
    {
        services.AddSingleton<IOutboxStore, T>();
        return services;
    }
}
