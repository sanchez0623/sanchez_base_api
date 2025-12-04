using Microsoft.Extensions.Options;

namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Sharding route rule for tenant-based database sharding.
/// Distributes entities across different databases based on tenant ID.
/// </summary>
/// <typeparam name="TEntity">The entity type being sharded.</typeparam>
public class TenantShardingRoute<TEntity> : IShardingRouteRule<TEntity> where TEntity : class
{
    private readonly ShardingOptions _options;
    private readonly TableShardingConfig? _tableConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantShardingRoute{TEntity}"/> class.
    /// </summary>
    /// <param name="options">The sharding options.</param>
    public TenantShardingRoute(IOptions<ShardingOptions> options)
    {
        _options = options.Value;
        var tableName = typeof(TEntity).Name;
        _options.Tables.TryGetValue(tableName, out _tableConfig);
    }

    /// <inheritdoc />
    public string GetTableSuffix(object shardingValue)
    {
        if (shardingValue is null)
        {
            return _options.DefaultDataSource;
        }

        return shardingValue.ToString() ?? _options.DefaultDataSource;
    }

    /// <inheritdoc />
    public string GetActualTableName(string logicalTableName, object shardingValue)
    {
        // For tenant sharding, the table name usually stays the same
        // but the database/connection changes based on tenant
        if (_tableConfig == null || string.IsNullOrEmpty(_tableConfig.TableNameFormat))
        {
            return logicalTableName;
        }

        var tenantId = shardingValue?.ToString() ?? _options.DefaultDataSource;
        return string.Format(_tableConfig.TableNameFormat, tenantId);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime)
    {
        // For tenant sharding, we typically only return the logical table name
        // since each tenant has their own database
        yield return logicalTableName;
    }

    /// <summary>
    /// Gets the data source (database) name for the given tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>The data source name for the tenant.</returns>
    public string GetDataSourceName(string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return _options.DefaultDataSource;
        }

        return $"tenant_{tenantId}";
    }
}
