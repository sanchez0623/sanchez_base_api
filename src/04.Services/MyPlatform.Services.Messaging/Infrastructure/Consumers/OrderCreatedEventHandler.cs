using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.Services.Messaging.Domain.Entities;
using MyPlatform.Services.Messaging.Domain.Events;
using MyPlatform.Services.Messaging.Infrastructure.Data;

namespace MyPlatform.Services.Messaging.Infrastructure.Consumers;

/// <summary>
/// 订单创建事件处理器
/// </summary>
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly MessagingDbContext _dbContext;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        MessagingDbContext dbContext,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing OrderCreatedEvent: OrderId={OrderId}, CustomerId={CustomerId}, TotalAmount={TotalAmount}",
            @event.OrderId, @event.CustomerId, @event.TotalAmount);

        // 幂等性检查：检查是否已处理过该事件
        var existingRecord = await _dbContext.ConsumeRecords
            .FirstOrDefaultAsync(r => 
                r.EventId == @event.EventId.ToString() && 
                r.ConsumerGroup == nameof(OrderCreatedEventHandler),
                cancellationToken);

        if (existingRecord is not null)
        {
            _logger.LogWarning(
                "Event {EventId} already processed by {ConsumerGroup}, skipping",
                @event.EventId, nameof(OrderCreatedEventHandler));
            return;
        }

        try
        {
            // ========================================
            // 业务逻辑处理
            // ========================================
            
            // 1. 发送订单确认通知（邮件/短信/推送）
            await SendOrderConfirmationNotificationAsync(@event, cancellationToken);

            // 2. 触发库存预留流程
            await TriggerInventoryReservationAsync(@event, cancellationToken);

            // 3. 记录审计日志
            await RecordAuditLogAsync(@event, cancellationToken);

            // 记录消费成功
            _dbContext.ConsumeRecords.Add(new MessageConsumeRecord
            {
                EventId = @event.EventId.ToString(),
                EventType = @event.EventType,
                ConsumerGroup = nameof(OrderCreatedEventHandler),
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully processed OrderCreatedEvent: OrderId={OrderId}",
                @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process OrderCreatedEvent: OrderId={OrderId}",
                @event.OrderId);
            throw;
        }
    }

    private Task SendOrderConfirmationNotificationAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现发送订单确认通知的逻辑
        // 可以集成邮件服务、短信服务或推送服务
        _logger.LogDebug("Sending order confirmation notification for OrderId={OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }

    private Task TriggerInventoryReservationAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现库存预留逻辑
        // 可以调用库存服务API或发布库存预留事件
        _logger.LogDebug("Triggering inventory reservation for OrderId={OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }

    private Task RecordAuditLogAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现审计日志记录
        _logger.LogDebug("Recording audit log for OrderId={OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}
