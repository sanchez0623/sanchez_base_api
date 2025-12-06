using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.Services.Messaging.Application.Dtos;
using MyPlatform.Services.Messaging.Domain.Events;

namespace MyPlatform.Services.Messaging.Application.Services;

/// <summary>
/// 事件发布应用服务
/// 自动从租户上下文中注入TenantId到每个事件
/// </summary>
public class EventPublishAppService
{
    private readonly ILogger<EventPublishAppService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITenantContext _tenantContext;

    public EventPublishAppService(
        ILogger<EventPublishAppService> logger,
        IEventPublisher eventPublisher,
        ITenantContext tenantContext)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// 发布订单创建事件（使用RabbitMQ，高可靠性场景）
    /// </summary>
    public async Task<PublishEventResponse> PublishOrderCreatedAsync(
        PublishOrderCreatedRequest request,
        CancellationToken cancellationToken = default)
    {
        var evt = new OrderCreatedEvent
        {
            TenantId = _tenantContext.TenantId, // 自动注入租户ID
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            TotalAmount = request.TotalAmount,
            Items = request.Items.Select(i => new OrderItemDto
            {
                SkuCode = i.SkuCode,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        
        _logger.LogInformation("订单创建事件已发布: TenantId={TenantId}, OrderId={OrderId}", 
            evt.TenantId, request.OrderId);

        return new PublishEventResponse
        {
            EventId = evt.EventId,
            Status = "Published",
            PublishedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 发布库存变更事件
    /// </summary>
    public async Task<PublishEventResponse> PublishInventoryChangedAsync(
        PublishInventoryChangedRequest request,
        CancellationToken cancellationToken = default)
    {
        var evt = new InventoryChangedEvent
        {
            TenantId = _tenantContext.TenantId, // 自动注入租户ID
            SkuCode = request.SkuCode,
            QuantityChange = request.QuantityChange,
            ChangeType = request.ChangeType,
            Reason = request.Reason
        };

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        
        _logger.LogInformation("库存变更事件已发布: TenantId={TenantId}, SkuCode={SkuCode}", 
            evt.TenantId, request.SkuCode);

        return new PublishEventResponse
        {
            EventId = evt.EventId,
            Status = "Published",
            PublishedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 发布物流状态更新事件
    /// </summary>
    public async Task<PublishEventResponse> PublishLogisticsStatusAsync(
        PublishLogisticsStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var evt = new LogisticsStatusEvent
        {
            TenantId = _tenantContext.TenantId, // 自动注入租户ID
            OrderId = request.OrderId,
            TrackingNumber = request.TrackingNumber,
            Status = request.Status,
            Location = request.Location,
            UpdatedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        
        _logger.LogInformation("物流状态事件已发布: TenantId={TenantId}, OrderId={OrderId}", 
            evt.TenantId, request.OrderId);

        return new PublishEventResponse
        {
            EventId = evt.EventId,
            Status = "Published",
            PublishedAt = DateTime.UtcNow
        };
    }
}
