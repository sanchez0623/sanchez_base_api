using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Data;

namespace MyPlatform.Services.Sku.Application.Services;

/// <summary>
/// 库存应用服务
/// </summary>
public class StockAppService
{
    private readonly SkuDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// 创建库存应用服务
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="tenantContext">租户上下文</param>
    public StockAppService(SkuDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// 获取SKU库存
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>库存信息</returns>
    public async Task<StockInfoDto?> GetStockAsync(long skuId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stock = await _dbContext.SkuStocks
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.TenantId == tenantId, cancellationToken);

        return stock is null ? null : new StockInfoDto
        {
            SkuId = stock.SkuId,
            AvailableQuantity = stock.AvailableQuantity,
            ReservedQuantity = stock.ReservedQuantity,
            SafetyStock = stock.SafetyStock,
            TotalQuantity = stock.GetTotalQuantity(),
            IsBelowSafetyStock = stock.IsBelowSafetyStock()
        };
    }

    /// <summary>
    /// 预留库存
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="quantity">预留数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> ReserveStockAsync(long skuId, int quantity, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stock = await _dbContext.SkuStocks
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.TenantId == tenantId, cancellationToken);

        if (stock is null)
        {
            return false;
        }

        if (!stock.Reserve(quantity))
        {
            return false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// 确认扣减库存
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="quantity">扣减数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> ConfirmDeductionAsync(long skuId, int quantity, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stock = await _dbContext.SkuStocks
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.TenantId == tenantId, cancellationToken);

        if (stock is null)
        {
            return false;
        }

        stock.ConfirmDeduction(quantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// 释放预留库存
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="quantity">释放数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> ReleaseReservationAsync(long skuId, int quantity, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stock = await _dbContext.SkuStocks
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.TenantId == tenantId, cancellationToken);

        if (stock is null)
        {
            return false;
        }

        stock.ReleaseReservation(quantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// 增加库存
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="quantity">增加数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> AddStockAsync(long skuId, int quantity, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stock = await _dbContext.SkuStocks
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.TenantId == tenantId, cancellationToken);

        if (stock is null)
        {
            return false;
        }

        stock.AddStock(quantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// 批量获取库存信息
    /// </summary>
    /// <param name="skuIds">SKU ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>库存信息列表</returns>
    public async Task<IReadOnlyList<StockInfoDto>> GetStocksBySkuIdsAsync(IEnumerable<long> skuIds, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        var stocks = await _dbContext.SkuStocks
            .Where(s => skuIds.Contains(s.SkuId) && s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return stocks.Select(s => new StockInfoDto
        {
            SkuId = s.SkuId,
            AvailableQuantity = s.AvailableQuantity,
            ReservedQuantity = s.ReservedQuantity,
            SafetyStock = s.SafetyStock,
            TotalQuantity = s.GetTotalQuantity(),
            IsBelowSafetyStock = s.IsBelowSafetyStock()
        }).ToList();
    }
}

/// <summary>
/// 库存信息DTO
/// </summary>
public class StockInfoDto
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public long SkuId { get; set; }

    /// <summary>
    /// 可用库存
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// 预留库存
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// 安全库存
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 总库存
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 是否低于安全库存
    /// </summary>
    public bool IsBelowSafetyStock { get; set; }
}
