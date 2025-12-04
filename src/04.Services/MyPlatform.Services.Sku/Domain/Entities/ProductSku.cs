using MyPlatform.Services.Sku.Domain.Events;
using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Services.Sku.Domain.Entities;

/// <summary>
/// SKU实体 - 按ID取模分4表
/// </summary>
public class ProductSku : AggregateRoot
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public long ProductId { get; private set; }

    /// <summary>
    /// SKU编码
    /// </summary>
    public string SkuCode { get; private set; } = string.Empty;

    /// <summary>
    /// SKU名称
    /// </summary>
    public string SkuName { get; private set; } = string.Empty;

    /// <summary>
    /// 销售价格
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 成本价格
    /// </summary>
    public decimal CostPrice { get; private set; }

    /// <summary>
    /// 属性JSON（如：{"color":"红色","size":"XL"}）
    /// </summary>
    public string? AttributesJson { get; private set; }

    /// <summary>
    /// SKU状态：0-禁用，1-启用
    /// </summary>
    public int Status { get; private set; }

    /// <summary>
    /// 用于EF Core
    /// </summary>
    protected ProductSku() { }

    /// <summary>
    /// 创建SKU
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="skuCode">SKU编码</param>
    /// <param name="skuName">SKU名称</param>
    /// <param name="price">销售价格</param>
    /// <param name="costPrice">成本价格</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="attributesJson">属性JSON</param>
    public ProductSku(
        long productId,
        string skuCode,
        string skuName,
        decimal price,
        decimal costPrice,
        string tenantId,
        string? attributesJson = null)
    {
        ProductId = productId;
        SkuCode = skuCode ?? throw new ArgumentNullException(nameof(skuCode));
        SkuName = skuName ?? throw new ArgumentNullException(nameof(skuName));
        Price = price;
        CostPrice = costPrice;
        AttributesJson = attributesJson;
        Status = 1; // 默认启用
        SetTenant(tenantId);
    }

    /// <summary>
    /// 更新价格
    /// </summary>
    /// <param name="newPrice">新价格</param>
    /// <param name="reason">变更原因</param>
    /// <param name="operatorId">操作人ID</param>
    public void UpdatePrice(decimal newPrice, string reason, string operatorId)
    {
        var oldPrice = Price;
        Price = newPrice;

        // 添加价格变更领域事件
        AddDomainEvent(new SkuPriceChangedEvent(
            Id,
            TenantId ?? string.Empty,
            oldPrice,
            newPrice,
            reason,
            operatorId));
    }

    /// <summary>
    /// 更新成本价
    /// </summary>
    /// <param name="newCostPrice">新成本价</param>
    public void UpdateCostPrice(decimal newCostPrice)
    {
        CostPrice = newCostPrice;
    }

    /// <summary>
    /// 启用SKU
    /// </summary>
    public void Enable()
    {
        Status = 1;
    }

    /// <summary>
    /// 禁用SKU
    /// </summary>
    public void Disable()
    {
        Status = 0;
    }

    /// <summary>
    /// 更新属性
    /// </summary>
    /// <param name="attributesJson">属性JSON</param>
    public void UpdateAttributes(string? attributesJson)
    {
        AttributesJson = attributesJson;
    }
}
