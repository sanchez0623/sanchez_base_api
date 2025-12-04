using Microsoft.EntityFrameworkCore;
using MyPlatform.Infrastructure.EFCore.Repositories;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Data;
using MyPlatform.Shared.Kernel.Repositories;

namespace MyPlatform.Services.Sku.Infrastructure.Repositories;

/// <summary>
/// 商品仓储接口
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// 根据分类获取商品
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(long categoryId, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据品牌获取商品
    /// </summary>
    /// <param name="brand">品牌</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    Task<IReadOnlyList<Product>> GetByBrandAsync(string brand, string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 商品仓储实现
/// </summary>
public class ProductRepository : EfCoreRepository<Product, SkuDbContext>, IProductRepository
{
    /// <summary>
    /// 创建商品仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="unitOfWork">工作单元</param>
    public ProductRepository(SkuDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(long categoryId, string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.CategoryId == categoryId && p.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByBrandAsync(string brand, string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Brand == brand && p.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
