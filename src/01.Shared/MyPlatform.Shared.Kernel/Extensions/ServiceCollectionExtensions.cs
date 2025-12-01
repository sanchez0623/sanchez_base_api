using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Shared.Kernel.Events;

namespace MyPlatform.Shared.Kernel.Extensions;

/// <summary>
/// Extension methods for registering Kernel services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Shared Kernel services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}
