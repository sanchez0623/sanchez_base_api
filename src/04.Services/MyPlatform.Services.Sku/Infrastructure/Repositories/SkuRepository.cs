using Microsoft.EntityFrameworkCore;
using MyPlatform.Infrastructure.EFCore.Extensions;
using MyPlatform.Infrastructure.EFCore.Repositories;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Data;
using MyPlatform.Shared.Kernel.Repositories;

namespace MyPlatform.Services.Sku.Infrastructure.Repositories;

/// <summary>
/// SKU仓储接口
/// </summary>
public interface ISkuRepository : IRepository<ProductSku>
{
    /// <summary>
    /// 根据商品ID获取SKU列表
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="useMaster">是否使用主库</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU列表</returns>
    Task<IReadOnlyList<ProductSku>> GetByProductIdAsync(long productId, string tenantId, bool useMaster = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据SKU编码获取SKU
    /// </summary>
    /// <param name="skuCode">SKU编码</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="useMaster">是否使用主库</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU实体</returns>
    Task<ProductSku?> GetBySkuCodeAsync(string skuCode, string tenantId, bool useMaster = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取SKU详情（强制读主库）
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU实体</returns>
    Task<ProductSku?> GetByIdFromMasterAsync(long id, CancellationToken cancellationToken = default);
}

/// <summary>
/// SKU仓储实现
/// </summary>
public class SkuRepository : EfCoreRepository<ProductSku, SkuDbContext>, ISkuRepository
{
    /// <summary>
    /// 创建SKU仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="unitOfWork">工作单元</param>
    public SkuRepository(SkuDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSku>> GetByProductIdAsync(long productId, string tenantId, bool useMaster = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(s => s.ProductId == productId && s.TenantId == tenantId);

        if (useMaster)
        {
            // 使用.UseMaster()强制读主库
            return await ReadWriteSplitExtensions.WithMasterAsync(async () =>
                await query.ToListAsync(cancellationToken));
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductSku?> GetBySkuCodeAsync(string skuCode, string tenantId, bool useMaster = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(s => s.SkuCode == skuCode && s.TenantId == tenantId);

        if (useMaster)
        {
            return await ReadWriteSplitExtensions.WithMasterAsync(async () =>
                await query.FirstOrDefaultAsync(cancellationToken));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductSku?> GetByIdFromMasterAsync(long id, CancellationToken cancellationToken = default)
    {
        // 演示使用.UseMaster()扩展方法强制读主库
        return await ReadWriteSplitExtensions.WithMasterAsync(async () =>
            await DbSet.FindAsync([id], cancellationToken));
    }
}
