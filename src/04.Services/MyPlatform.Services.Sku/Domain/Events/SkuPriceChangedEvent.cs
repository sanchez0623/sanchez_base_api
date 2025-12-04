using MyPlatform.Shared.Kernel.Events;

namespace MyPlatform.Services.Sku.Domain.Events;

/// <summary>
/// 价格变更领域事件
/// </summary>
public class SkuPriceChangedEvent : DomainEvent
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public long SkuId { get; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// 原价格
    /// </summary>
    public decimal OldPrice { get; }

    /// <summary>
    /// 新价格
    /// </summary>
    public decimal NewPrice { get; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// 操作人ID
    /// </summary>
    public string OperatorId { get; }

    /// <summary>
    /// 创建价格变更事件
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="oldPrice">原价格</param>
    /// <param name="newPrice">新价格</param>
    /// <param name="reason">变更原因</param>
    /// <param name="operatorId">操作人ID</param>
    public SkuPriceChangedEvent(
        long skuId,
        string tenantId,
        decimal oldPrice,
        decimal newPrice,
        string reason,
        string operatorId)
    {
        SkuId = skuId;
        TenantId = tenantId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Reason = reason;
        OperatorId = operatorId;
    }
}
