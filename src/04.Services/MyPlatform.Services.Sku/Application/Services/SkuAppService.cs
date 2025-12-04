using Microsoft.EntityFrameworkCore;
using MyPlatform.Infrastructure.EFCore.Extensions;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Services.Sku.Application.Dtos;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Data;
using MyPlatform.Services.Sku.Infrastructure.Repositories;

namespace MyPlatform.Services.Sku.Application.Services;

/// <summary>
/// SKU应用服务
/// 展示读写分离和分库分表的使用场景
/// </summary>
public class SkuAppService
{
    private readonly ISkuRepository _skuRepository;
    private readonly SkuDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// 创建SKU应用服务
    /// </summary>
    /// <param name="skuRepository">SKU仓储</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="tenantContext">租户上下文</param>
    public SkuAppService(
        ISkuRepository skuRepository,
        SkuDbContext dbContext,
        ITenantContext tenantContext)
    {
        _skuRepository = skuRepository;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// 获取SKU列表
    /// 读操作，自动走从库
    /// </summary>
    /// <param name="productId">商品ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU列表</returns>
    public async Task<IReadOnlyList<SkuDto>> GetSkuListAsync(long? productId = null, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        // 读操作自动走从库
        var query = _dbContext.ProductSkus
            .Where(s => s.TenantId == tenantId);

        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        var skus = await query.ToListAsync(cancellationToken);
        return skus.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 获取SKU详情
    /// 支持useMaster参数强制读主库
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="useMaster">是否强制读主库</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    public async Task<SkuDto?> GetSkuByIdAsync(long id, bool useMaster = false, CancellationToken cancellationToken = default)
    {
        ProductSku? sku;

        if (useMaster)
        {
            // 使用WithMasterAsync强制读主库
            sku = await _skuRepository.GetByIdFromMasterAsync(id, cancellationToken);
        }
        else
        {
            // 默认读从库
            sku = await _skuRepository.GetByIdAsync(id, cancellationToken);
        }

        return sku is null ? null : MapToDto(sku);
    }

    /// <summary>
    /// 创建SKU
    /// 写操作，自动走主库
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    public async Task<SkuDto> CreateSkuAsync(CreateSkuRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        // 创建SKU实体
        var sku = new ProductSku(
            request.ProductId,
            request.SkuCode,
            request.SkuName,
            request.Price,
            request.CostPrice,
            tenantId,
            request.AttributesJson);

        // 写操作自动走主库
        await _skuRepository.AddAsync(sku, cancellationToken);

        // 创建初始库存
        var stock = new SkuStock(
            sku.Id,
            tenantId,
            request.InitialStock,
            request.SafetyStock);

        _dbContext.SkuStocks.Add(stock);

        await _skuRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(sku);
    }

    /// <summary>
    /// 更新SKU价格
    /// 写操作 + 记录价格历史（时间分表）
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="request">更新价格请求</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    public async Task<SkuDto?> UpdateSkuPriceAsync(long id, UpdatePriceRequest request, string operatorId, CancellationToken cancellationToken = default)
    {
        // 使用强制读主库，确保获取最新数据
        var sku = await _skuRepository.GetByIdFromMasterAsync(id, cancellationToken);
        if (sku is null)
        {
            return null;
        }

        var oldPrice = sku.Price;

        // 更新价格，这会触发领域事件
        sku.UpdatePrice(request.NewPrice, request.Reason, operatorId);
        _skuRepository.Update(sku);

        // 创建价格历史记录（按月分表）
        var priceHistory = new SkuPriceHistory(
            sku.Id,
            sku.TenantId ?? string.Empty,
            oldPrice,
            request.NewPrice,
            request.Reason,
            operatorId);

        _dbContext.SkuPriceHistories.Add(priceHistory);

        await _skuRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(sku);
    }

    /// <summary>
    /// 获取价格变更历史
    /// 支持跨月查询（时间分表）
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>价格历史列表</returns>
    public async Task<IReadOnlyList<PriceHistoryDto>> GetPriceHistoryAsync(
        long skuId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        // 跨月查询价格历史
        var query = _dbContext.SkuPriceHistories
            .Where(h => h.SkuId == skuId && h.TenantId == tenantId);

        if (startDate.HasValue)
        {
            query = query.Where(h => h.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.CreatedAt <= endDate.Value);
        }

        var histories = await query
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        return histories.Select(h => new PriceHistoryDto
        {
            Id = h.Id,
            SkuId = h.SkuId,
            OldPrice = h.OldPrice,
            NewPrice = h.NewPrice,
            Reason = h.Reason,
            OperatorId = h.OperatorId,
            CreatedAt = h.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// 批量获取SKU（演示跨分片查询）
    /// </summary>
    /// <param name="skuIds">SKU ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU列表</returns>
    public async Task<IReadOnlyList<SkuDto>> GetSkusByIdsAsync(IEnumerable<long> skuIds, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;

        // 跨分片查询
        var skus = await _dbContext.ProductSkus
            .Where(s => skuIds.Contains(s.Id) && s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return skus.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    /// <param name="sku">SKU实体</param>
    /// <returns>SKU DTO</returns>
    private static SkuDto MapToDto(ProductSku sku)
    {
        return new SkuDto
        {
            Id = sku.Id,
            TenantId = sku.TenantId,
            ProductId = sku.ProductId,
            SkuCode = sku.SkuCode,
            SkuName = sku.SkuName,
            Price = sku.Price,
            CostPrice = sku.CostPrice,
            AttributesJson = sku.AttributesJson,
            Status = sku.Status,
            CreatedAt = sku.CreatedAt,
            UpdatedAt = sku.UpdatedAt
        };
    }
}
