namespace MyPlatform.Services.Messaging.Domain.Entities;

/// <summary>
/// 消息发布记录（用于幂等性和审计）
/// </summary>
public class MessagePublishRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Published, Failed
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// 消息消费记录（用于幂等性）
/// </summary>
public class MessageConsumeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ConsumerGroup { get; set; } = string.Empty;
    public string Status { get; set; } = "Processed"; // Processed, Failed
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
