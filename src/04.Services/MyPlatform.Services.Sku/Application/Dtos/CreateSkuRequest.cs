namespace MyPlatform.Services.Sku.Application.Dtos;

/// <summary>
/// 创建SKU请求
/// </summary>
public class CreateSkuRequest
{
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
    /// 初始库存
    /// </summary>
    public int InitialStock { get; set; }

    /// <summary>
    /// 安全库存
    /// </summary>
    public int SafetyStock { get; set; }
}
