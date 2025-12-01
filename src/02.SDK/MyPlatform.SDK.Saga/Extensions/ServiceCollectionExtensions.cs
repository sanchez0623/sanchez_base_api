using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Saga.Configuration;
using MyPlatform.SDK.Saga.Persistence;

namespace MyPlatform.SDK.Saga.Extensions;

/// <summary>
/// Extension methods for registering Saga services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Saga orchestration services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformSaga(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SagaOptions>(configuration.GetSection("Saga"));
        services.AddSingleton<ISagaStateStore, InMemorySagaStateStore>();

        return services;
    }

    /// <summary>
    /// Adds Saga orchestration services with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure saga options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformSaga(this IServiceCollection services, Action<SagaOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddSingleton<ISagaStateStore, InMemorySagaStateStore>();

        return services;
    }

    /// <summary>
    /// Adds a custom saga state store implementation.
    /// </summary>
    /// <typeparam name="T">The type of saga state store.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSagaStateStore<T>(this IServiceCollection services)
        where T : class, ISagaStateStore
    {
        services.AddSingleton<ISagaStateStore, T>();
        return services;
    }

    /// <summary>
    /// Registers a saga orchestrator.
    /// </summary>
    /// <typeparam name="TOrchestrator">The type of saga orchestrator.</typeparam>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSagaOrchestrator<TOrchestrator, TData>(this IServiceCollection services)
        where TOrchestrator : class, Orchestration.ISagaOrchestrator<TData>
        where TData : class, new()
    {
        services.AddScoped<Orchestration.ISagaOrchestrator<TData>, TOrchestrator>();
        return services;
    }
}
