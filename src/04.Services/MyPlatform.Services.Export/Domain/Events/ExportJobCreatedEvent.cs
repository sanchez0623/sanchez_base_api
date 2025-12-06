using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.Services.Export.Domain.Events;

/// <summary>
/// 导出作业创建事件
/// 通过消息队列（RabbitMQ/Kafka）分发给后台Worker
/// </summary>
public class ExportJobCreatedEvent : IntegrationEvent
{
    /// <summary>
    /// 作业ID
    /// </summary>
    public Guid JobId { get; set; }
    
    /// <summary>
    /// 期望版本（乐观锁）
    /// </summary>
    public int ExpectedVersion { get; set; }
}
