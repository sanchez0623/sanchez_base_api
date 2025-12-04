using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

namespace MyPlatform.Infrastructure.EFCore.Extensions;

/// <summary>
/// Extension methods for configuring read-write split functionality.
/// </summary>
public static class ReadWriteSplitExtensions
{
    /// <summary>
    /// Adds read-write split support to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddReadWriteSplit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReadWriteOptions>(
            configuration.GetSection(ReadWriteOptions.SectionName));

        services.AddSingleton<IConnectionStringResolver, ReadWriteConnectionStringResolver>();

        return services;
    }

    /// <summary>
    /// Adds read-write split support with custom options configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddReadWriteSplit(
        this IServiceCollection services,
        Action<ReadWriteOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IConnectionStringResolver, ReadWriteConnectionStringResolver>();

        return services;
    }

    /// <summary>
    /// Forces the query to use the master database.
    /// This is useful when you need to read data that was just written.
    /// <para>
    /// <b>Warning:</b> This sets the ForceMaster flag until explicitly reset.
    /// Consider using <see cref="WithMaster{T}"/> or <see cref="WithMasterAsync{T}"/> 
    /// for automatic cleanup.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The query to modify.</param>
    /// <returns>The same query (force master is applied via async local context).</returns>
    /// <remarks>
    /// Call <see cref="ResetForceMaster"/> after executing the query to reset the flag.
    /// </remarks>
    public static IQueryable<T> UseMaster<T>(this IQueryable<T> query) where T : class
    {
        ReadWriteDbCommandInterceptor.ForceMaster = true;
        return query;
    }

    /// <summary>
    /// Executes an action with forced master database usage.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <returns>The result of the action.</returns>
    public static T WithMaster<T>(Func<T> action)
    {
        var previousValue = ReadWriteDbCommandInterceptor.ForceMaster;
        try
        {
            ReadWriteDbCommandInterceptor.ForceMaster = true;
            return action();
        }
        finally
        {
            ReadWriteDbCommandInterceptor.ForceMaster = previousValue;
        }
    }

    /// <summary>
    /// Executes an async action with forced master database usage.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task representing the result of the action.</returns>
    public static async Task<T> WithMasterAsync<T>(Func<Task<T>> action)
    {
        var previousValue = ReadWriteDbCommandInterceptor.ForceMaster;
        try
        {
            ReadWriteDbCommandInterceptor.ForceMaster = true;
            return await action();
        }
        finally
        {
            ReadWriteDbCommandInterceptor.ForceMaster = previousValue;
        }
    }

    /// <summary>
    /// Resets the force master flag.
    /// </summary>
    public static void ResetForceMaster()
    {
        ReadWriteDbCommandInterceptor.ForceMaster = false;
    }
}
