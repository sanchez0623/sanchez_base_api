using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Authorization.Handlers;
using MyPlatform.SDK.Authorization.Services;

namespace MyPlatform.SDK.Authorization.Extensions;

/// <summary>
/// Extension methods for registering Authorization services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RBAC authorization services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Adds a custom permission checker implementation.
    /// </summary>
    /// <typeparam name="T">The type of permission checker.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPermissionChecker<T>(this IServiceCollection services)
        where T : class, IPermissionChecker
    {
        services.AddScoped<IPermissionChecker, T>();
        return services;
    }

    /// <summary>
    /// Adds a custom role permission service implementation.
    /// </summary>
    /// <typeparam name="T">The type of role permission service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRolePermissionService<T>(this IServiceCollection services)
        where T : class, IRolePermissionService
    {
        services.AddScoped<IRolePermissionService, T>();
        return services;
    }
}
