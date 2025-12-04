using Microsoft.Extensions.Options;

namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Sharding route rule for date/time-based table sharding.
/// Supports sharding by year, month, or day.
/// </summary>
/// <typeparam name="TEntity">The entity type being sharded.</typeparam>
public class DateTimeShardingRoute<TEntity> : IShardingRouteRule<TEntity> where TEntity : class
{
    private readonly ShardingOptions _options;
    private readonly TableShardingConfig? _tableConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeShardingRoute{TEntity}"/> class.
    /// </summary>
    /// <param name="options">The sharding options.</param>
    public DateTimeShardingRoute(IOptions<ShardingOptions> options)
    {
        _options = options.Value;
        var tableName = typeof(TEntity).Name;
        _options.Tables.TryGetValue(tableName, out _tableConfig);
    }

    /// <inheritdoc />
    public string GetTableSuffix(object shardingValue)
    {
        var dateTime = ConvertToDateTime(shardingValue);
        if (!dateTime.HasValue || _tableConfig == null)
        {
            return string.Empty;
        }

        return _tableConfig.ShardingType switch
        {
            ShardingType.ByYear => dateTime.Value.ToString("yyyy"),
            ShardingType.ByMonth => dateTime.Value.ToString("yyyyMM"),
            ShardingType.ByDay => dateTime.Value.ToString("yyyyMMdd"),
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public string GetActualTableName(string logicalTableName, object shardingValue)
    {
        if (_tableConfig == null || string.IsNullOrEmpty(_tableConfig.TableNameFormat))
        {
            return logicalTableName;
        }

        var dateTime = ConvertToDateTime(shardingValue);
        if (!dateTime.HasValue)
        {
            return logicalTableName;
        }

        return string.Format(_tableConfig.TableNameFormat, dateTime.Value);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime)
    {
        if (_tableConfig == null || string.IsNullOrEmpty(_tableConfig.TableNameFormat))
        {
            yield return logicalTableName;
            yield break;
        }

        var start = startTime ?? DateTime.UtcNow.AddMonths(-_options.DefaultLookBackMonths);
        var end = endTime ?? DateTime.UtcNow;

        var current = start;
        while (current <= end)
        {
            yield return GetActualTableName(logicalTableName, current);

            current = _tableConfig.ShardingType switch
            {
                ShardingType.ByYear => current.AddYears(1),
                ShardingType.ByMonth => current.AddMonths(1),
                ShardingType.ByDay => current.AddDays(1),
                _ => current.AddMonths(1)
            };
        }
    }

    /// <summary>
    /// Converts the sharding value to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The DateTime value, or null if conversion fails.</returns>
    private static DateTime? ConvertToDateTime(object value)
    {
        return value switch
        {
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime,
            string dateString when DateTime.TryParse(dateString, out var parsed) => parsed,
            long ticks => new DateTime(ticks),
            _ => null
        };
    }
}
