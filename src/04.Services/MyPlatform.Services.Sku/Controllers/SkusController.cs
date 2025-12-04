using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPlatform.Services.Sku.Application.Dtos;
using MyPlatform.Services.Sku.Application.Services;

namespace MyPlatform.Services.Sku.Controllers;

/// <summary>
/// SKU控制器
/// 展示读写分离和分库分表的使用
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkusController : ControllerBase
{
    private readonly SkuAppService _skuAppService;
    private readonly StockAppService _stockAppService;

    /// <summary>
    /// 创建SKU控制器
    /// </summary>
    /// <param name="skuAppService">SKU应用服务</param>
    /// <param name="stockAppService">库存应用服务</param>
    public SkusController(SkuAppService skuAppService, StockAppService stockAppService)
    {
        _skuAppService = skuAppService;
        _stockAppService = stockAppService;
    }

    /// <summary>
    /// 获取SKU列表
    /// 读操作，自动走从库
    /// </summary>
    /// <param name="productId">商品ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU列表</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SkuDto>>> GetSkus([FromQuery] long? productId, CancellationToken cancellationToken)
    {
        var skus = await _skuAppService.GetSkuListAsync(productId, cancellationToken);
        return Ok(skus);
    }

    /// <summary>
    /// 获取SKU详情
    /// 支持useMaster参数强制读主库
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="useMaster">是否强制读主库（用于刚写入后立即读取的场景）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SkuDto>> GetSku(long id, [FromQuery] bool useMaster = false, CancellationToken cancellationToken = default)
    {
        var sku = await _skuAppService.GetSkuByIdAsync(id, useMaster, cancellationToken);
        if (sku is null)
        {
            return NotFound();
        }
        return Ok(sku);
    }

    /// <summary>
    /// 创建SKU
    /// 写操作，自动走主库
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    [HttpPost]
    public async Task<ActionResult<SkuDto>> CreateSku([FromBody] CreateSkuRequest request, CancellationToken cancellationToken)
    {
        var sku = await _skuAppService.CreateSkuAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSku), new { id = sku.Id }, sku);
    }

    /// <summary>
    /// 更新SKU价格
    /// 写操作，会记录价格历史（按月分表）
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="request">更新价格请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU DTO</returns>
    [HttpPut("{id:long}/price")]
    public async Task<ActionResult<SkuDto>> UpdatePrice(long id, [FromBody] UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        var operatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var sku = await _skuAppService.UpdateSkuPriceAsync(id, request, operatorId, cancellationToken);
        if (sku is null)
        {
            return NotFound();
        }
        return Ok(sku);
    }

    /// <summary>
    /// 获取价格变更历史
    /// 支持跨月查询（时间分表场景）
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>价格历史列表</returns>
    [HttpGet("{id:long}/price-history")]
    public async Task<ActionResult<IReadOnlyList<PriceHistoryDto>>> GetPriceHistory(
        long id,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var histories = await _skuAppService.GetPriceHistoryAsync(id, startDate, endDate, cancellationToken);
        return Ok(histories);
    }

    /// <summary>
    /// 批量获取SKU
    /// 演示跨分片查询
    /// </summary>
    /// <param name="ids">SKU ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>SKU列表</returns>
    [HttpGet("batch")]
    public async Task<ActionResult<IReadOnlyList<SkuDto>>> GetSkusByIds([FromQuery] long[] ids, CancellationToken cancellationToken)
    {
        var skus = await _skuAppService.GetSkusByIdsAsync(ids, cancellationToken);
        return Ok(skus);
    }

    /// <summary>
    /// 获取SKU库存
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>库存信息</returns>
    [HttpGet("{id:long}/stock")]
    public async Task<ActionResult<StockInfoDto>> GetStock(long id, CancellationToken cancellationToken)
    {
        var stock = await _stockAppService.GetStockAsync(id, cancellationToken);
        if (stock is null)
        {
            return NotFound();
        }
        return Ok(stock);
    }

    /// <summary>
    /// 预留库存
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="quantity">预留数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id:long}/stock/reserve")]
    public async Task<IActionResult> ReserveStock(long id, [FromQuery] int quantity, CancellationToken cancellationToken)
    {
        var result = await _stockAppService.ReserveStockAsync(id, quantity, cancellationToken);
        if (!result)
        {
            return BadRequest("库存不足或SKU不存在");
        }
        return NoContent();
    }

    /// <summary>
    /// 增加库存
    /// </summary>
    /// <param name="id">SKU ID</param>
    /// <param name="quantity">增加数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id:long}/stock/add")]
    public async Task<IActionResult> AddStock(long id, [FromQuery] int quantity, CancellationToken cancellationToken)
    {
        var result = await _stockAppService.AddStockAsync(id, quantity, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
