namespace MyPlatform.Services.Worker.Consumers;

/// <summary>
/// 示例事件消费者 - 演示消息消费者模式
/// </summary>
/// <remarks>
/// 此服务展示了如何创建一个消息消费者:
/// - 继承 BackgroundService
/// - 连接消息队列并订阅消息
/// - 处理接收到的消息
/// - 正确处理连接断开和重连
/// 
/// 典型使用场景:
/// - 订阅订单创建事件，进行后续处理
/// - 订阅用户注册事件，发送欢迎邮件
/// - 订阅支付完成事件，更新库存
/// 
/// 实际使用时，需要配置 RabbitMQ 连接并实现消息处理逻辑:
/// <code>
/// // 在 appsettings.json 中配置
/// {
///   "RabbitMQ": {
///     "HostName": "localhost",
///     "Port": 5672,
///     "UserName": "guest",
///     "Password": "guest",
///     "ExchangeName": "myplatform.events"
///   }
/// }
/// </code>
/// </remarks>
public class SampleEventConsumer : BackgroundService
{
    private readonly ILogger<SampleEventConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 初始化事件消费者
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="serviceProvider">服务提供者</param>
    public SampleEventConsumer(
        ILogger<SampleEventConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 执行消费者主循环
    /// </summary>
    /// <param name="stoppingToken">停止令牌</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("事件消费者服务已启动");

        // ============================================================
        // 在实际项目中，这里应该:
        // 1. 从 RabbitMQ 连接创建 Channel
        // 2. 声明队列并绑定到交换机
        // 3. 设置消息消费回调
        // 
        // 示例代码（需要 RabbitMQ.Client）:
        // 
        // var factory = new ConnectionFactory
        // {
        //     HostName = _options.HostName,
        //     Port = _options.Port,
        //     UserName = _options.UserName,
        //     Password = _options.Password
        // };
        // 
        // using var connection = factory.CreateConnection();
        // using var channel = connection.CreateModel();
        // 
        // channel.QueueDeclare(
        //     queue: "worker.events",
        //     durable: true,
        //     exclusive: false,
        //     autoDelete: false);
        // 
        // channel.QueueBind(
        //     queue: "worker.events",
        //     exchange: _options.ExchangeName,
        //     routingKey: "#");
        // 
        // var consumer = new EventingBasicConsumer(channel);
        // consumer.Received += async (model, ea) =>
        // {
        //     await HandleMessageAsync(ea.Body.ToArray(), stoppingToken);
        //     channel.BasicAck(ea.DeliveryTag, false);
        // };
        // 
        // channel.BasicConsume(queue: "worker.events", autoAck: false, consumer: consumer);
        // ============================================================

        // ============================================================
        // 注意：以下为演示代码
        // 在实际项目中，请使用上述注释中的 RabbitMQ 消费者实现
        // 替换下面的模拟循环逻辑
        // ============================================================

        // 模拟消费者运行（仅用于演示）
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 模拟接收和处理消息
                await SimulateMessageProcessingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // 服务正在停止，正常退出
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事件消费者发生错误，将在5秒后重试");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("事件消费者服务已停止");
    }

    /// <summary>
    /// 模拟消息处理（仅用于演示）
    /// </summary>
    private async Task SimulateMessageProcessingAsync(CancellationToken cancellationToken)
    {
        // 等待模拟的消息到达
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

        // 模拟接收到消息
        var messageId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogDebug("收到模拟消息, MessageId: {MessageId}", messageId);

        // 处理消息
        await HandleMessageAsync(messageId, "SampleEvent", "{}", cancellationToken);
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">消息内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task HandleMessageAsync(
        string messageId,
        string eventType,
        string payload,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "开始处理消息, MessageId: {MessageId}, EventType: {EventType}",
            messageId,
            eventType);

        try
        {
            // ============================================================
            // 创建作用域以获取 scoped 服务
            // 这对于处理需要数据库访问或租户上下文的消息很重要
            // ============================================================
            using var scope = _serviceProvider.CreateScope();

            // 在这里实现具体的消息处理逻辑
            // 例如:
            // var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            // await orderService.ProcessOrderEventAsync(payload, cancellationToken);

            // 模拟处理时间
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

            _logger.LogInformation(
                "消息处理完成, MessageId: {MessageId}",
                messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "消息处理失败, MessageId: {MessageId}, EventType: {EventType}",
                messageId,
                eventType);

            // ============================================================
            // 根据异常类型决定是否重试
            // - 临时性错误：抛出异常，消息将被重新入队
            // - 永久性错误：记录日志，消息进入死信队列
            // ============================================================
            throw;
        }
    }
}
