namespace MyPlatform.Services.Messaging.Application.Dtos;

/// <summary>
/// 发送订单创建事件请求
/// </summary>
public class PublishOrderCreatedRequest
{
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public string SkuCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// 发送库存变更事件请求
/// </summary>
public class PublishInventoryChangedRequest
{
    public string SkuCode { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public string ChangeType { get; set; } = "OUT";
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 发送物流状态事件请求
/// </summary>
public class PublishLogisticsStatusRequest
{
    public long OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// 事件发布响应
/// </summary>
public class PublishEventResponse
{
    public Guid EventId { get; set; }
    public string Status { get; set; } = "Published";
    public DateTime PublishedAt { get; set; }
}
