namespace MyPlatform.Services.Sku.Application.Dtos;

/// <summary>
/// 商品DTO
/// </summary>
public class ProductDto
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// 商品状态
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
/// 创建商品请求
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }
}

/// <summary>
/// 更新商品请求
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }
}
