using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.Services.Messaging.Domain.Events;

/// <summary>
/// 订单创建事件
/// </summary>
public class OrderCreatedEvent : IntegrationEvent
{
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// 订单项DTO
/// </summary>
public class OrderItemDto
{
    public string SkuCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// 库存变更事件
/// </summary>
public class InventoryChangedEvent : IntegrationEvent
{
    public string SkuCode { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // "IN" or "OUT"
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 物流状态更新事件
/// </summary>
public class LogisticsStatusEvent : IntegrationEvent
{
    public long OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
