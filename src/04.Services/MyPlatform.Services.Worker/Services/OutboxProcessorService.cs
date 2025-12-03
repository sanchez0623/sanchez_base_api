using Microsoft.Extensions.Options;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.Outbox;
using System.Text.Json;

namespace MyPlatform.Services.Worker.Services;

/// <summary>
/// Outbox 处理器配置选项
/// </summary>
public class OutboxOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Outbox";

    /// <summary>
    /// 每批处理的消息数量
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 处理间隔（秒）
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 已处理消息保留天数
    /// </summary>
    public int RetentionDays { get; set; } = 7;
}

/// <summary>
/// Outbox 消息处理器服务 - 演示 Outbox 模式的实现
/// </summary>
/// <remarks>
/// 此服务展示了如何实现可靠的消息发布:
/// - 继承 BackgroundService
/// - 定期扫描未处理的 Outbox 消息
/// - 发布消息到消息队列
/// - 处理失败重试和清理
/// 
/// Outbox 模式的优势:
/// - 保证事务一致性（业务操作和消息写入在同一事务）
/// - 保证消息至少投递一次
/// - 支持失败重试
/// 
/// 典型使用场景:
/// - 订单创建后发送订单事件
/// - 用户注册后发送欢迎邮件
/// - 支付完成后发送通知
/// </remarks>
public class OutboxProcessorService : BackgroundService
{
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly IOutboxStore _outboxStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly OutboxOptions _options;

    /// <summary>
    /// 初始化 Outbox 处理器服务
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="outboxStore">Outbox 存储</param>
    /// <param name="eventPublisher">事件发布者</param>
    /// <param name="options">配置选项</param>
    public OutboxProcessorService(
        ILogger<OutboxProcessorService> logger,
        IOutboxStore outboxStore,
        IEventPublisher eventPublisher,
        IOptions<OutboxOptions> options)
    {
        _logger = logger;
        _outboxStore = outboxStore;
        _eventPublisher = eventPublisher;
        _options = options.Value;
    }

    /// <summary>
    /// 执行后台服务主循环
    /// </summary>
    /// <param name="stoppingToken">停止令牌</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox 处理器服务已启动, 处理间隔: {Interval}秒, 批次大小: {BatchSize}",
            _options.ProcessingIntervalSeconds,
            _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
                await CleanupProcessedMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // 服务正在停止，正常退出
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox 处理器发生错误");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds),
                stoppingToken);
        }

        _logger.LogInformation("Outbox 处理器服务已停止");
    }

    /// <summary>
    /// 处理未发送的 Outbox 消息
    /// </summary>
    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        var messages = await _outboxStore.GetUnprocessedAsync(_options.BatchSize, cancellationToken);
        var messageList = messages.ToList();

        if (messageList.Count == 0)
        {
            return;
        }

        _logger.LogDebug("发现 {Count} 条待处理的 Outbox 消息", messageList.Count);

        foreach (var message in messageList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                // 检查重试次数
                if (message.RetryCount >= _options.MaxRetryCount)
                {
                    _logger.LogWarning(
                        "消息 {MessageId} 已达到最大重试次数 {MaxRetry}, 标记为失败",
                        message.Id,
                        _options.MaxRetryCount);

                    await _outboxStore.MarkAsFailedAsync(
                        message.Id,
                        $"超过最大重试次数 {_options.MaxRetryCount}",
                        cancellationToken);
                    continue;
                }

                // 发布事件
                await PublishEventAsync(message, cancellationToken);

                // 标记为已处理
                await _outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);

                _logger.LogDebug(
                    "消息 {MessageId} 发布成功, 类型: {EventType}",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "消息 {MessageId} 发布失败, 重试次数: {RetryCount}",
                    message.Id,
                    message.RetryCount + 1);

                await _outboxStore.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 发布事件到消息队列
    /// </summary>
    private async Task PublishEventAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        // ============================================================
        // 根据事件类型反序列化并发布
        // 实际项目中应该使用事件类型注册表来处理
        // ============================================================

        _logger.LogDebug(
            "正在发布事件, MessageId: {MessageId}, EventType: {EventType}, TenantId: {TenantId}",
            message.Id,
            message.EventType,
            message.TenantId);

        // 这里模拟发布操作
        // 实际实现中，需要根据 EventType 反序列化 Payload 并调用 IEventPublisher
        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
    }

    /// <summary>
    /// 清理已处理的历史消息
    /// </summary>
    private async Task CleanupProcessedMessagesAsync(CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        var deletedCount = await _outboxStore.DeleteProcessedAsync(cutoffDate, cancellationToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "已清理 {Count} 条过期的 Outbox 消息, 截止日期: {CutoffDate:yyyy-MM-dd}",
                deletedCount,
                cutoffDate);
        }
    }
}
