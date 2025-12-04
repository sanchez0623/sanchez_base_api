using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.MultiTenancy.Configuration;
using MyPlatform.SDK.MultiTenancy.Exceptions;
using MyPlatform.SDK.MultiTenancy.Models;
using MyPlatform.SDK.MultiTenancy.Services;

namespace MyPlatform.SDK.MultiTenancy.DataSource;

/// <summary>
/// Factory for creating tenant-specific DbContext instances.
/// </summary>
/// <typeparam name="TContext">The type of DbContext.</typeparam>
public class TenantDbContextFactory<TContext> : ITenantDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ITenantConnectionStringResolver _connectionStringResolver;
    private readonly IDbContextFactory<TContext>? _innerFactory;
    private readonly Func<DbContextOptions<TContext>, TContext>? _contextFactory;
    private readonly MultiTenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory{TContext}"/> class.
    /// </summary>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="connectionStringResolver">The connection string resolver.</param>
    /// <param name="options">The multi-tenancy options.</param>
    /// <param name="contextFactory">Optional factory function to create the DbContext.</param>
    public TenantDbContextFactory(
        ITenantContext tenantContext,
        ITenantConnectionStringResolver connectionStringResolver,
        IOptions<MultiTenancyOptions> options,
        Func<DbContextOptions<TContext>, TContext>? contextFactory = null)
    {
        _tenantContext = tenantContext;
        _connectionStringResolver = connectionStringResolver;
        _options = options.Value;
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory{TContext}"/> class
    /// with an inner factory.
    /// </summary>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="connectionStringResolver">The connection string resolver.</param>
    /// <param name="options">The multi-tenancy options.</param>
    /// <param name="innerFactory">The inner DbContext factory.</param>
    public TenantDbContextFactory(
        ITenantContext tenantContext,
        ITenantConnectionStringResolver connectionStringResolver,
        IOptions<MultiTenancyOptions> options,
        IDbContextFactory<TContext> innerFactory)
    {
        _tenantContext = tenantContext;
        _connectionStringResolver = connectionStringResolver;
        _options = options.Value;
        _innerFactory = innerFactory;
    }

    /// <inheritdoc />
    public TContext CreateDbContext()
    {
        var currentTenant = GetCurrentTenant();
        var connectionString = _connectionStringResolver.GetConnectionString(currentTenant);

        return CreateContextWithConnectionString(connectionString);
    }

    /// <inheritdoc />
    public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }

    private TenantInfo GetCurrentTenant()
    {
        var currentTenant = _tenantContext.CurrentTenant;

        if (currentTenant == null)
        {
            if (string.IsNullOrEmpty(_tenantContext.TenantId))
            {
                throw new InvalidOperationException("No tenant is currently set in the context.");
            }

            throw new TenantNotFoundException(_tenantContext.TenantId);
        }

        if (currentTenant.Status == TenantStatus.Suspended)
        {
            throw new TenantSuspendedException(currentTenant.TenantId);
        }

        if (currentTenant.Status == TenantStatus.Deleted)
        {
            throw new TenantNotFoundException(currentTenant.TenantId);
        }

        return currentTenant;
    }

    private TContext CreateContextWithConnectionString(string connectionString)
    {
        if (_innerFactory != null)
        {
            return _innerFactory.CreateDbContext();
        }

        if (_contextFactory == null)
        {
            throw new InvalidOperationException(
                "No DbContext factory configured. Either provide a IDbContextFactory<TContext> or a factory function.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        ConfigureDbContext(optionsBuilder, connectionString);

        return _contextFactory(optionsBuilder.Options);
    }

    /// <summary>
    /// Configures the DbContext options with the specified connection string.
    /// Override this method to customize the database provider configuration.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="connectionString">The connection string.</param>
    protected virtual void ConfigureDbContext(DbContextOptionsBuilder<TContext> optionsBuilder, string connectionString)
    {
        // Default implementation - subclasses should override to configure specific providers
        // Example for MySQL: optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        // Example for SqlServer: optionsBuilder.UseSqlServer(connectionString);
    }
}
