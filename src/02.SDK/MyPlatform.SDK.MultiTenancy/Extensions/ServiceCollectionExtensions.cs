using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.MultiTenancy.Middleware;
using MyPlatform.SDK.MultiTenancy.Resolvers;
using MyPlatform.SDK.MultiTenancy.Services;

namespace MyPlatform.SDK.MultiTenancy.Extensions;

/// <summary>
/// Extension methods for registering MultiTenancy services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-tenancy services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // Add default resolvers
        services.AddSingleton<ITenantResolver, HeaderTenantResolver>();
        services.AddSingleton<ITenantResolver, ClaimTenantResolver>();

        return services;
    }

    /// <summary>
    /// Adds a custom tenant resolver.
    /// </summary>
    /// <typeparam name="T">The type of tenant resolver.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantResolver<T>(this IServiceCollection services)
        where T : class, ITenantResolver
    {
        services.AddSingleton<ITenantResolver, T>();
        return services;
    }
}

/// <summary>
/// Extension methods for configuring multi-tenancy middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the tenant resolution middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
