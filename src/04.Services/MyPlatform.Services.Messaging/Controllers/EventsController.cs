using Microsoft.AspNetCore.Mvc;
using MyPlatform.Services.Messaging.Application.Dtos;
using MyPlatform.Services.Messaging.Application.Services;

namespace MyPlatform.Services.Messaging.Controllers;

/// <summary>
/// 消息事件控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventPublishAppService _eventService;

    public EventsController(EventPublishAppService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// 发布订单创建事件
    /// </summary>
    [HttpPost("order-created")]
    public async Task<ActionResult<PublishEventResponse>> PublishOrderCreated(
        [FromBody] PublishOrderCreatedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _eventService.PublishOrderCreatedAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// 发布库存变更事件
    /// </summary>
    [HttpPost("inventory-changed")]
    public async Task<ActionResult<PublishEventResponse>> PublishInventoryChanged(
        [FromBody] PublishInventoryChangedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _eventService.PublishInventoryChangedAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// 发布物流状态事件
    /// </summary>
    [HttpPost("logistics-status")]
    public async Task<ActionResult<PublishEventResponse>> PublishLogisticsStatus(
        [FromBody] PublishLogisticsStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _eventService.PublishLogisticsStatusAsync(request, cancellationToken);
        return Ok(response);
    }
}
