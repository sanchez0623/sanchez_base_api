using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Services.Sku.Application.Dtos;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Repositories;

namespace MyPlatform.Services.Sku.Application.Services;

/// <summary>
/// 商品应用服务
/// </summary>
public class ProductAppService
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// 创建商品应用服务
    /// </summary>
    /// <param name="productRepository">商品仓储</param>
    /// <param name="tenantContext">租户上下文</param>
    public ProductAppService(IProductRepository productRepository, ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// 获取商品列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    public async Task<IReadOnlyList<ProductDto>> GetProductListAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken: cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 获取商品详情
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    public async Task<ProductDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : MapToDto(product);
    }

    /// <summary>
    /// 根据分类获取商品
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    public async Task<IReadOnlyList<ProductDto>> GetProductsByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;
        var products = await _productRepository.GetByCategoryAsync(categoryId, tenantId, cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? string.Empty;
        var product = new Product(
            request.Name,
            request.CategoryId,
            tenantId,
            request.Description,
            request.Brand);

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    public async Task<ProductDto?> UpdateProductAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Update(request.Name, request.Description, request.Brand);
        _productRepository.Update(product);
        await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    /// <summary>
    /// 上架商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> PublishProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Publish();
        _productRepository.Update(product);
        await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 下架商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> UnpublishProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Unpublish();
        _productRepository.Update(product);
        await _productRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    /// <param name="product">商品实体</param>
    /// <returns>商品DTO</returns>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            TenantId = product.TenantId,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Brand = product.Brand,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
