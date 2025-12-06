
using System.Threading.Tasks;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.SDK.EventBus.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace MyPlatform.Services.EventDemo
{
    /// <summary>
    /// Scenario 1: Order Processing / 场景1：订单处理
    /// Technology: RabbitMQ
    /// Reasoning: Requires high reliability (ACKs), Dead Letter Queues (DLQ) for failed retries, and transactional safety.
    /// 选择理由：需要高可靠性（ACK机制）、死信队列（DLQ）重试机制，以及事务安全性。
    /// </summary>
    public class OrderProcessor
    {
        // Inject generalized IEventPublisher. 
        // In startup, this should be mapped to RabbitMqEventPublisher.
        // 注入通用 IEventPublisher。在启动配置中，应将其映射到 RabbitMqEventPublisher。
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderProcessor> _logger;

        public OrderProcessor(IEventPublisher eventPublisher, ILogger<OrderProcessor> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task PlaceOrder(long orderId, string userId)
        {
            // ... Business Logic (DB Save) ...
            // ... 业务逻辑（保存数据库）...
            _logger.LogInformation("Order {OrderId} saved to DB.", orderId);

            var orderEvt = new OrderCreatedEvent
            {
                OrderId = orderId,
                UserId = userId,
                TotalAmount = 99.99m
            };

            // Publish to RabbitMQ (Exchange defined in options, RoutingKey = OrderCreatedEvent)
            // 发布到 RabbitMQ（Exchange在配置中定义，RoutingKey = OrderCreatedEvent）
            await _eventPublisher.PublishAsync(orderEvt);
            
            _logger.LogInformation("OrderCreatedEvent published to RabbitMQ.");
        }
    }
}
