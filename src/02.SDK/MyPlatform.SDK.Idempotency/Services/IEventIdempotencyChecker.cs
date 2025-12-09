namespace MyPlatform.SDK.Idempotency.Services;

/// <summary>
/// 事件消费者幂等性检查器接口
/// 用于消息队列消费者场景，确保事件不被重复处理
/// </summary>
/// <remarks>
/// 与 IIdempotencyService 的区别：
/// - IIdempotencyService: 用于 HTTP API 请求幂等，存储完整 HTTP 响应
/// - IEventIdempotencyChecker: 用于消息消费者幂等，仅存储状态标记，支持失败重试
/// </remarks>
public interface IEventIdempotencyChecker
{
    /// <summary>
    /// 尝试获取处理权（原子操作）
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="consumerGroup">消费者组名称</param>
    /// <param name="expiry">过期时间，默认7天</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>true=获取成功可以处理，false=已被其他消费者处理</returns>
    Task<bool> TryAcquireAsync(
        string eventId,
        string consumerGroup,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记处理完成
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="consumerGroup">消费者组名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkCompletedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记处理失败（允许后续重试）
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="consumerGroup">消费者组名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkFailedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查事件是否已成功处理
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="consumerGroup">消费者组名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>true=已成功处理，false=未处理或处理失败</returns>
    Task<bool> IsProcessedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件当前的处理状态
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="consumerGroup">消费者组名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理状态，未找到返回 null</returns>
    Task<EventProcessingStatus?> GetStatusAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 事件处理状态
/// </summary>
public enum EventProcessingStatus
{
    /// <summary>
    /// 处理中
    /// </summary>
    Processing,

    /// <summary>
    /// 处理完成
    /// </summary>
    Completed,

    /// <summary>
    /// 处理失败（可重试）
    /// </summary>
    Failed
}
