
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.EventBus.Kafka;

namespace MyPlatform.Services.EventDemo
{
    /// <summary>
    /// Example of how to use KafkaEventSubscriber.
    /// 这里的类继承自 KafkaEventSubscriber，它会自动作为后台服务运行。
    /// </summary>
    public class InventoryEventConsumer : KafkaEventSubscriber
    {
        private readonly ILogger<InventoryEventConsumer> _logger;

        public InventoryEventConsumer(ILogger<InventoryEventConsumer> logger)
            // BootstrapServers: Kafka address / Kafka地址
            // GroupId: "inventory-group" -> Independent consumer group / 独立的消费组
            // Topic: "inventory.updates" -> The topic to listen to / 监听的主题
            : base(bootstrapServers: "localhost:9092", groupId: "inventory-group", topic: "inventory.updates")
        {
            _logger = logger;
        }

        /// <summary>
        /// This method is called automatically when a message arrives.
        /// 当收到消息时，此方法会被自动调用。
        /// </summary>
        protected override void ProcessMessage(string message)
        {
            // In a real app, you would deserialize this JSON.
            // 在真实应用中，你会反序列化这个JSON。
            _logger.LogInformation("Received Inventory Update via Kafka: {Message}", message);
            
            // Handle business logic...
            // 处理业务逻辑...
        }
    }
}
