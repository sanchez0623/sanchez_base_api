using Microsoft.Extensions.Options;
using MyPlatform.Infrastructure.EFCore.Sharding;
using MyPlatform.Services.Sku.Domain.Entities;

namespace MyPlatform.Services.Sku.Infrastructure.Sharding;

/// <summary>
/// 价格历史时间分表路由
/// 按月分表
/// </summary>
public class PriceHistoryTimeShardingRoute : IShardingRouteRule<SkuPriceHistory>
{
    private readonly ShardingOptions _options;
    private readonly TableShardingConfig? _tableConfig;

    /// <summary>
    /// 创建价格历史分表路由
    /// </summary>
    /// <param name="options">分片配置</param>
    public PriceHistoryTimeShardingRoute(IOptions<ShardingOptions> options)
    {
        _options = options.Value;
        _options.Tables.TryGetValue(nameof(SkuPriceHistory), out _tableConfig);
    }

    /// <inheritdoc />
    public string GetTableSuffix(object shardingValue)
    {
        var dateTime = ConvertToDateTime(shardingValue);
        return dateTime.ToString("yyyyMM");
    }

    /// <inheritdoc />
    public string GetActualTableName(string logicalTableName, object shardingValue)
    {
        return $"{logicalTableName}_{GetTableSuffix(shardingValue)}";
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime)
    {
        var start = startTime ?? DateTime.UtcNow.AddYears(-1);
        var end = endTime ?? DateTime.UtcNow;
        var current = new DateTime(start.Year, start.Month, 1);

        while (current <= end)
        {
            yield return $"{logicalTableName}_{current:yyyyMM}";
            current = current.AddMonths(1);
        }
    }

    /// <summary>
    /// 转换为DateTime类型
    /// </summary>
    /// <param name="value">值</param>
    /// <returns>DateTime值</returns>
    private static DateTime ConvertToDateTime(object value)
    {
        return value switch
        {
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime,
            string dateString when DateTime.TryParse(dateString, out var parsed) => parsed,
            _ => DateTime.UtcNow
        };
    }
}
