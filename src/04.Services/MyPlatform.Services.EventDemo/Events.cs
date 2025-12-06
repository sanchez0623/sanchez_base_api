
using System;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.Shared.Contracts.Events;

namespace MyPlatform.Services.EventDemo
{
    // 1. Order Created (Critical Business Event)
    // 订单创建事件（关键业务事件）
    public class OrderCreatedEvent : IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        public long OrderId { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // 2. Inventory Changed (High Volume Event)
    // 库存变更事件（高容量事件）
    public class InventoryChangedEvent : IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        public string SkuCode { get; set; }
        public int QuantityDelta { get; set; }
        public long WarehouseId { get; set; }
    }

    // 3. Logistics Status (Stream Data)
    // 物流状态更新（流数据）
    public class LogisticsStatusEvent : IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        public string TrackingNumber { get; set; }
        public string Status { get; set; } // e.g. "PickedUp", "InTransit"
        public string Location { get; set; }
    }
}
