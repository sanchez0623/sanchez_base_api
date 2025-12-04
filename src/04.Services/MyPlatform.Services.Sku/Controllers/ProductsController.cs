using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPlatform.Services.Sku.Application.Dtos;
using MyPlatform.Services.Sku.Application.Services;

namespace MyPlatform.Services.Sku.Controllers;

/// <summary>
/// 商品控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductAppService _productAppService;

    /// <summary>
    /// 创建商品控制器
    /// </summary>
    /// <param name="productAppService">商品应用服务</param>
    public ProductsController(ProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    /// <summary>
    /// 获取商品列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _productAppService.GetProductListAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// 获取商品详情
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDto>> GetProduct(long id, CancellationToken cancellationToken)
    {
        var product = await _productAppService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// 根据分类获取商品
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品列表</returns>
    [HttpGet("by-category/{categoryId:long}")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProductsByCategory(long categoryId, CancellationToken cancellationToken)
    {
        var products = await _productAppService.GetProductsByCategoryAsync(categoryId, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productAppService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商品DTO</returns>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(long id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productAppService.UpdateProductAsync(id, request, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// 上架商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id:long}/publish")]
    public async Task<IActionResult> PublishProduct(long id, CancellationToken cancellationToken)
    {
        var result = await _productAppService.PublishProductAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// 下架商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id:long}/unpublish")]
    public async Task<IActionResult> UnpublishProduct(long id, CancellationToken cancellationToken)
    {
        var result = await _productAppService.UnpublishProductAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
