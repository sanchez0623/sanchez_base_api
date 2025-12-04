using Microsoft.Extensions.Options;
using MyPlatform.Infrastructure.EFCore.Sharding;
using MyPlatform.Services.Sku.Domain.Entities;

namespace MyPlatform.Services.Sku.Infrastructure.Sharding;

/// <summary>
/// SKU取模分表路由
/// 按SKU ID取模分4表
/// </summary>
public class SkuModShardingRoute : IShardingRouteRule<ProductSku>
{
    private const int ShardCount = 4;
    private readonly ShardingOptions _options;
    private readonly TableShardingConfig? _tableConfig;

    /// <summary>
    /// 创建SKU分表路由
    /// </summary>
    /// <param name="options">分片配置</param>
    public SkuModShardingRoute(IOptions<ShardingOptions> options)
    {
        _options = options.Value;
        _options.Tables.TryGetValue(nameof(ProductSku), out _tableConfig);
    }

    /// <inheritdoc />
    public string GetTableSuffix(object shardingValue)
    {
        var id = ConvertToLong(shardingValue);
        return (id % ShardCount).ToString();
    }

    /// <inheritdoc />
    public string GetActualTableName(string logicalTableName, object shardingValue)
    {
        return $"{logicalTableName}_{GetTableSuffix(shardingValue)}";
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime)
    {
        for (int i = 0; i < ShardCount; i++)
        {
            yield return $"{logicalTableName}_{i}";
        }
    }

    /// <summary>
    /// 转换为long类型
    /// </summary>
    /// <param name="value">值</param>
    /// <returns>long值</returns>
    private static long ConvertToLong(object value)
    {
        return value switch
        {
            long l => l,
            int i => i,
            string str when long.TryParse(str, out var parsed) => parsed,
            _ => 0
        };
    }
}
