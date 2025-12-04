using Microsoft.Extensions.Options;

namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Sharding route rule for modulo-based table sharding.
/// Distributes entities across a fixed number of tables based on ID modulo.
/// </summary>
/// <typeparam name="TEntity">The entity type being sharded.</typeparam>
public class ModShardingRoute<TEntity> : IShardingRouteRule<TEntity> where TEntity : class
{
    private readonly ShardingOptions _options;
    private readonly TableShardingConfig? _tableConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModShardingRoute{TEntity}"/> class.
    /// </summary>
    /// <param name="options">The sharding options.</param>
    public ModShardingRoute(IOptions<ShardingOptions> options)
    {
        _options = options.Value;
        var tableName = typeof(TEntity).Name;
        _options.Tables.TryGetValue(tableName, out _tableConfig);
    }

    /// <inheritdoc />
    public string GetTableSuffix(object shardingValue)
    {
        if (_tableConfig == null || _tableConfig.ShardCount <= 0)
        {
            return string.Empty;
        }

        var numericValue = ConvertToLong(shardingValue);
        if (!numericValue.HasValue)
        {
            return "0";
        }

        var shardIndex = Math.Abs(numericValue.Value % _tableConfig.ShardCount);
        return shardIndex.ToString();
    }

    /// <inheritdoc />
    public string GetActualTableName(string logicalTableName, object shardingValue)
    {
        if (_tableConfig == null || string.IsNullOrEmpty(_tableConfig.TableNameFormat))
        {
            return logicalTableName;
        }

        var suffix = GetTableSuffix(shardingValue);
        return string.Format(_tableConfig.TableNameFormat, suffix);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime)
    {
        if (_tableConfig == null || string.IsNullOrEmpty(_tableConfig.TableNameFormat))
        {
            yield return logicalTableName;
            yield break;
        }

        var shardCount = _tableConfig.ShardCount > 0 ? _tableConfig.ShardCount : 4;
        for (var i = 0; i < shardCount; i++)
        {
            yield return string.Format(_tableConfig.TableNameFormat, i);
        }
    }

    /// <summary>
    /// Converts the sharding value to a long.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The long value, or null if conversion fails.</returns>
    private static long? ConvertToLong(object value)
    {
        return value switch
        {
            long l => l,
            int i => i,
            short s => s,
            byte b => b,
            ulong ul => (long)ul,
            uint ui => ui,
            ushort us => us,
            string str when long.TryParse(str, out var parsed) => parsed,
            Guid guid => BitConverter.ToInt64(guid.ToByteArray(), 0),
            _ => null
        };
    }
}
