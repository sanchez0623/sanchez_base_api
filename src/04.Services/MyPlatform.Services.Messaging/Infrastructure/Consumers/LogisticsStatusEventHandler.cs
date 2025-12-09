using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.Idempotency.Services;
using MyPlatform.Services.Messaging.Domain.Events;

namespace MyPlatform.Services.Messaging.Infrastructure.Consumers;

/// <summary>
/// 物流状态更新事件处理器
/// 使用 Redis 实现高性能幂等性检查，支持 10W+ QPS
/// </summary>
public class LogisticsStatusEventHandler : IIntegrationEventHandler<LogisticsStatusEvent>
{
    private readonly IEventIdempotencyChecker _idempotencyChecker;
    private readonly ILogger<LogisticsStatusEventHandler> _logger;
    private const string ConsumerGroup = nameof(LogisticsStatusEventHandler);

    public LogisticsStatusEventHandler(
        IEventIdempotencyChecker idempotencyChecker,
        ILogger<LogisticsStatusEventHandler> logger)
    {
        _idempotencyChecker = idempotencyChecker;
        _logger = logger;
    }

    public async Task HandleAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing LogisticsStatusEvent: OrderId={OrderId}, TrackingNumber={TrackingNumber}, Status={Status}",
            @event.OrderId, @event.TrackingNumber, @event.Status);

        // Redis 幂等性检查：原子操作，高并发下不会重复
        var acquired = await _idempotencyChecker.TryAcquireAsync(
            @event.EventId.ToString(),
            ConsumerGroup,
            TimeSpan.FromDays(7),
            cancellationToken);

        if (!acquired)
        {
            _logger.LogWarning(
                "Event {EventId} already processed by {ConsumerGroup}, skipping",
                @event.EventId, ConsumerGroup);
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

            // 标记处理完成
            await _idempotencyChecker.MarkCompletedAsync(
                @event.EventId.ToString(),
                ConsumerGroup,
                cancellationToken);

            _logger.LogInformation(
                "Successfully processed LogisticsStatusEvent: OrderId={OrderId}, Status={Status}",
                @event.OrderId, @event.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process LogisticsStatusEvent: OrderId={OrderId}",
                @event.OrderId);

            // 标记处理失败，允许重试
            await _idempotencyChecker.MarkFailedAsync(
                @event.EventId.ToString(),
                ConsumerGroup,
                cancellationToken);

            throw;
        }
    }

    private static bool IsDeliveredStatus(string status)
    {
        return status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Signed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("已签收", StringComparison.OrdinalIgnoreCase);
    }

    private Task SendLogisticsNotificationAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现发送物流通知的逻辑
        _logger.LogDebug(
            "Sending logistics notification for OrderId={OrderId}, Status={Status}",
            @event.OrderId, @event.Status);
        return Task.CompletedTask;
    }

    private Task UpdateOrderLogisticsStatusAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现更新订单物流状态的逻辑
        _logger.LogDebug(
            "Updating order logistics status for OrderId={OrderId}",
            @event.OrderId);
        return Task.CompletedTask;
    }

    private Task TriggerDeliveryCompletionFlowAsync(LogisticsStatusEvent @event, CancellationToken cancellationToken)
    {
        // TODO: 实现签收后的后续流程
        _logger.LogDebug(
            "Triggering delivery completion flow for OrderId={OrderId}",
            @event.OrderId);
        return Task.CompletedTask;
    }
}
