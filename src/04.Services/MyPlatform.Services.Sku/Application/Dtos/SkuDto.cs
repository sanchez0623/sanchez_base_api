namespace MyPlatform.Services.Sku.Application.Dtos;

/// <summary>
/// SKU DTO
/// </summary>
public class SkuDto
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    /// SKU编码
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>
    /// SKU名称
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// 销售价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 成本价格
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 属性JSON
    /// </summary>
    public string? AttributesJson { get; set; }

    /// <summary>
    /// SKU状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 价格历史DTO
/// </summary>
public class PriceHistoryDto
{
    /// <summary>
    /// 记录ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// SKU ID
    /// </summary>
    public long SkuId { get; set; }

    /// <summary>
    /// 原价格
    /// </summary>
    public decimal OldPrice { get; set; }

    /// <summary>
    /// 新价格
    /// </summary>
    public decimal NewPrice { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 操作人ID
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
