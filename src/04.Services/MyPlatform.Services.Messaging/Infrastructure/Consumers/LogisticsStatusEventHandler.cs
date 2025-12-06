using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.Services.Messaging.Domain.Entities;
using MyPlatform.Services.Messaging.Domain.Events;
using MyPlatform.Services.Messaging.Infrastructure.Data;

namespace MyPlatform.Services.Messaging.Infrastructure.Consumers;

/// <summary>
/// 物流状态更新事件处理器
/// </summary>
public class LogisticsStatusEventHandler : IIntegrationEventHandler<LogisticsStatusEvent>
{
    private readonly MessagingDbContext _dbContext;
    private readonly ILogger<LogisticsStatusEventHandler> _logger;

    public LogisticsStatusEventHandler(
        MessagingDbContext dbContext,
        ILogger<LogisticsStatusEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing LogisticsStatusEvent: OrderId={OrderId}, TrackingNumber={TrackingNumber}, Status={Status}",
            @event.OrderId, @event.TrackingNumber, @event.Status);

        // 幂等性检查
        var existingRecord = await _dbContext.ConsumeRecords
            .FirstOrDefaultAsync(r => 
                r.EventId == @event.EventId.ToString() && 
                r.ConsumerGroup == nameof(LogisticsStatusEventHandler),
                cancellationToken);

        if (existingRecord is not null)
        {
            _logger.LogWarning(
                "Event {EventId} already processed by {ConsumerGroup}, skipping",
                @event.EventId, nameof(LogisticsStatusEventHandler));
            return;
        }

        try
        {
            // ========================================
            // 业务逻辑处理
            // ========================================

            // 1. 发送物流状态更新通知给用户
            await SendLogisticsNotificationAsync(@event, cancellationToken);

            // 2. 更新订单的物流状态
            await UpdateOrderLogisticsStatusAsync(@event, cancellationToken);

            // 3. 如果是已签收状态，触发后续流程
            if (IsDeliveredStatus(@event.Status))
            {
                await TriggerDeliveryCompletionFlowAsync(@event, cancellationToken);
            }

            // 记录消费成功
            _dbContext.ConsumeRecords.Add(new MessageConsumeRecord
            {
                EventId = @event.EventId.ToString(),
                EventType = @event.EventType,
                ConsumerGroup = nameof(LogisticsStatusEventHandler),
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully processed LogisticsStatusEvent: OrderId={OrderId}, Status={Status}",
                @event.OrderId, @event.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process LogisticsStatusEvent: OrderId={OrderId}",
                @event.OrderId);
            throw;
        }
    }

    private bool IsDeliveredStatus(string status)
    {
        return status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Signed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("已签收", StringComparison.OrdinalIgnoreCase);
    }

    private Task SendLogisticsNotificationAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现发送物流通知的逻辑
        // 可以通过短信、推送或邮件通知用户
        _logger.LogDebug(
            "Sending logistics notification for OrderId={OrderId}, Status={Status}",
            @event.OrderId, @event.Status);
        return Task.CompletedTask;
    }

    private Task UpdateOrderLogisticsStatusAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现更新订单物流状态的逻辑
        // 可以调用订单服务API更新状态
        _logger.LogDebug(
            "Updating order logistics status for OrderId={OrderId}",
            @event.OrderId);
        return Task.CompletedTask;
    }

    private Task TriggerDeliveryCompletionFlowAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现签收后的后续流程
        // 例如：自动确认收货、开始售后期计算、发送评价邀请等
        _logger.LogDebug(
            "Triggering delivery completion flow for OrderId={OrderId}",
            @event.OrderId);
        return Task.CompletedTask;
    }
}
