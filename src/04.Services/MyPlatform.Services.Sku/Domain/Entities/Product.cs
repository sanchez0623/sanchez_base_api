using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Services.Sku.Domain.Entities;

/// <summary>
/// 商品实体 - 主表，不分表
/// </summary>
public class Product : AggregateRoot
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; private set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; private set; }

    /// <summary>
    /// 商品状态：0-下架，1-上架，2-审核中
    /// </summary>
    public int Status { get; private set; }

    /// <summary>
    /// 用于EF Core
    /// </summary>
    protected Product() { }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <param name="categoryId">分类ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="description">商品描述</param>
    /// <param name="brand">品牌</param>
    public Product(string name, long categoryId, string tenantId, string? description = null, string? brand = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CategoryId = categoryId;
        Description = description;
        Brand = brand;
        Status = 0; // 默认下架
        SetTenant(tenantId);
    }

    /// <summary>
    /// 更新商品信息
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <param name="description">商品描述</param>
    /// <param name="brand">品牌</param>
    public void Update(string name, string? description, string? brand)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Brand = brand;
    }

    /// <summary>
    /// 上架商品
    /// </summary>
    public void Publish()
    {
        Status = 1;
    }

    /// <summary>
    /// 下架商品
    /// </summary>
    public void Unpublish()
    {
        Status = 0;
    }

    /// <summary>
    /// 设置为审核中
    /// </summary>
    public void SetPending()
    {
        Status = 2;
    }
}
