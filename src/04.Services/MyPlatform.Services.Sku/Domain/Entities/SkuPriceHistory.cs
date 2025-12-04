using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Services.Sku.Domain.Entities;

/// <summary>
/// 价格变更历史 - 按CreatedAt按月分表
/// </summary>
public class SkuPriceHistory : Entity
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; private set; } = string.Empty;

    /// <summary>
    /// SKU ID
    /// </summary>
    public long SkuId { get; private set; }

    /// <summary>
    /// 原价格
    /// </summary>
    public decimal OldPrice { get; private set; }

    /// <summary>
    /// 新价格
    /// </summary>
    public decimal NewPrice { get; private set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// 操作人ID
    /// </summary>
    public string OperatorId { get; private set; } = string.Empty;

    /// <summary>
    /// 用于EF Core
    /// </summary>
    protected SkuPriceHistory() { }

    /// <summary>
    /// 创建价格历史记录
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="oldPrice">原价格</param>
    /// <param name="newPrice">新价格</param>
    /// <param name="reason">变更原因</param>
    /// <param name="operatorId">操作人ID</param>
    public SkuPriceHistory(
        long skuId,
        string tenantId,
        decimal oldPrice,
        decimal newPrice,
        string reason,
        string operatorId)
    {
        SkuId = skuId;
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Reason = reason ?? string.Empty;
        OperatorId = operatorId ?? throw new ArgumentNullException(nameof(operatorId));
    }

    /// <summary>
    /// 获取价格变动幅度
    /// </summary>
    /// <returns>变动幅度（正数为涨价，负数为降价）</returns>
    public decimal GetPriceChange()
    {
        return NewPrice - OldPrice;
    }

    /// <summary>
    /// 获取价格变动百分比
    /// </summary>
    /// <returns>变动百分比</returns>
    public decimal GetPriceChangePercentage()
    {
        if (OldPrice == 0)
        {
            return NewPrice > 0 ? 100 : 0;
        }

        return (NewPrice - OldPrice) / OldPrice * 100;
    }
}
