
using System.Threading.Tasks;
using MyPlatform.SDK.EventBus.Kafka;
using MyPlatform.SDK.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace MyPlatform.Services.EventDemo
{
    /// <summary>
    /// Scenario 2: Inventory Sync / 场景2：库存同步
    /// Technology: Kafka
    /// Reasoning: High Throughput. Inventory updates can happen thousands of times per second across global warehouses.
    /// 选择理由：高吞吐量。全球仓库的库存更新每秒可能发生数千次。
    /// </summary>
    public class InventorySyncer
    {
        // Explicitly use Kafka publisher or manage via Keyed dependency injection
        // 显式使用 Kafka 发布者或通过 Keyed DI 管理
        private readonly KafkaEventPublisher _kafkaPublisher;
        private readonly ILogger<InventorySyncer> _logger;

        public InventorySyncer(KafkaEventPublisher kafkaPublisher, ILogger<InventorySyncer> logger)
        {
            _kafkaPublisher = kafkaPublisher;
            _logger = logger;
        }

        public async Task SyncInventory(string sku, int quantityStart, int quantityEnd)
        {
            var invEvent = new InventoryChangedEvent
            {
                SkuCode = sku,
                QuantityDelta = quantityEnd - quantityStart,
                WarehouseId = 101
            };

            // Publish to Kafka topic "inventory.updates"
            // 发布到 Kafka Topic "inventory.updates"
            // Kafka is optimized for stream consumption by data warehouses or analytics.
            // Kafka 针对数据仓库或分析系统的流式消费进行了优化。
            await _kafkaPublisher.PublishAsync(invEvent, topic: "inventory.updates");
            
            _logger.LogInformation("Inventory event sent to Kafka.");
        }
    }

    /// <summary>
    /// Scenario 3: Logistics Status Updates / 场景3：物流状态更新
    /// Technology: Kafka
    /// Reasoning: Stream Processing. Logistics updates are a stream of events (Pickup -> Sort -> Transit -> Deliver).
    /// 选择理由：流处理。物流更新是一系列流事件（揽件 -> 分拣 -> 运输 -> 派送）。
    /// </summary>
    public class LogisticsStreamProcessor
    {
        private readonly KafkaEventPublisher _kafkaPublisher;

        public LogisticsStreamProcessor(KafkaEventPublisher kafkaPublisher)
        {
            _kafkaPublisher = kafkaPublisher;
        }

        public async Task UpdateStatus(string trackingNo, string status, string location)
        {
            var logEvent = new LogisticsStatusEvent
            {
                TrackingNumber = trackingNo,
                Status = status,
                Location = location
            };

            // High throughput stream
            await _kafkaPublisher.PublishAsync(logEvent, topic: "logistics.stream");
        }
    }
}
