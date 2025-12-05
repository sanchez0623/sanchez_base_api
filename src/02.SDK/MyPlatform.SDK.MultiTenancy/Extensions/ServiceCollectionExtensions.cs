using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.MultiTenancy.Configuration;
using MyPlatform.SDK.MultiTenancy.DataSource;
using MyPlatform.SDK.MultiTenancy.Middleware;
using MyPlatform.SDK.MultiTenancy.Resolvers;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.SDK.MultiTenancy.Store;

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
    /// Adds multi-tenancy services with configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// By default, this method registers either ConfigurationTenantStore or InMemoryTenantStore
    /// based on the TenantStore configuration value. These implementations are intended for
    /// development and testing environments.
    /// 
    /// For production environments, implement a custom ITenantStore that loads tenant information
    /// from a database and register it using <see cref="AddTenantStore{TStore}"/>.
    /// 
    /// Example:
    /// <code>
    /// services.AddPlatformMultiTenancy(configuration)
    ///         .AddTenantStore&lt;MySqlTenantStore&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddPlatformMultiTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<MultiTenancyOptions>(configuration.GetSection(MultiTenancyOptions.SectionName));

        // Add base services
        services.AddPlatformMultiTenancy();

        // Add memory cache for tenant info caching
        services.AddMemoryCache();

        // Get options to determine store type
        var options = new MultiTenancyOptions();
        configuration.GetSection(MultiTenancyOptions.SectionName).Bind(options);

        // Register tenant store based on configuration (built-in implementations only)
        switch (options.TenantStore?.ToLowerInvariant())
        {
            case "inmemory":
                // InMemory store for testing purposes
                services.TryAddScoped<ITenantStore, InMemoryTenantStore>();
                break;
            case "configuration":
            default:
                // Configuration store for development purposes
                // For production, use AddTenantStore<TStore>() to register a database-backed implementation
                services.TryAddScoped<ITenantStore, ConfigurationTenantStore>();
                break;
        }

        // Add caching decorator if enabled
        if (options.CacheTenantInfo)
        {
            services.Decorate<ITenantStore, CachedTenantStoreDecorator>();
        }

        // Add connection string resolver
        services.TryAddScoped<ITenantConnectionStringResolver, TenantConnectionStringResolver>();

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

    /// <summary>
    /// Registers a custom tenant store implementation.
    /// </summary>
    /// <typeparam name="TStore">The type of tenant store implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Use this method to register a custom ITenantStore implementation for production environments.
    /// This replaces any previously registered ITenantStore (e.g., ConfigurationTenantStore).
    /// 
    /// Example usage:
    /// <code>
    /// // Register your database context for tenant management
    /// services.AddDbContext&lt;TenantManagementDbContext&gt;(options => 
    ///     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    /// 
    /// // Register multi-tenancy with custom store
    /// services.AddPlatformMultiTenancy(configuration)
    ///         .AddTenantStore&lt;MySqlTenantStore&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddTenantStore<TStore>(this IServiceCollection services)
        where TStore : class, ITenantStore
    {
        // Remove any previously registered ITenantStore
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenantStore));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped<ITenantStore, TStore>();
        return services;
    }

    /// <summary>
    /// Adds a tenant DbContext factory for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantDbContextFactory<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.TryAddScoped<ITenantDbContextFactory<TContext>, TenantDbContextFactory<TContext>>();
        return services;
    }

    /// <summary>
    /// Adds a tenant DbContext factory with a custom factory function.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="contextFactory">The factory function to create DbContext instances.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantDbContextFactory<TContext>(
        this IServiceCollection services,
        Func<DbContextOptions<TContext>, TContext> contextFactory)
        where TContext : DbContext
    {
        services.AddScoped<ITenantDbContextFactory<TContext>>(sp =>
        {
            var tenantContext = sp.GetRequiredService<ITenantContext>();
            var connectionStringResolver = sp.GetRequiredService<ITenantConnectionStringResolver>();
            var options = sp.GetRequiredService<IOptions<MultiTenancyOptions>>();
            return new TenantDbContextFactory<TContext>(tenantContext, connectionStringResolver, options, contextFactory);
        });
        return services;
    }

    /// <summary>
    /// Decorates a service with a decorator implementation.
    /// </summary>
    private static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
    {
        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));

        if (wrappedDescriptor == null)
        {
            throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");
        }

        var objectFactory = ActivatorUtilities.CreateFactory(
            typeof(TDecorator),
            new[] { typeof(TService) });

        services.Replace(ServiceDescriptor.Describe(
            typeof(TService),
            sp => (TService)objectFactory(sp, new[] { CreateInstance(sp, wrappedDescriptor) }),
            wrappedDescriptor.Lifetime));

        return services;
    }

    private static object CreateInstance(IServiceProvider sp, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory != null)
        {
            return descriptor.ImplementationFactory(sp);
        }

        return ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);
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
