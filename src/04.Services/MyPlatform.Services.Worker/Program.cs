using MyPlatform.SDK.Caching.Extensions;
using MyPlatform.SDK.Core.Extensions;
using MyPlatform.SDK.EventBus.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using MyPlatform.SDK.Scheduler.Extensions;
using MyPlatform.Infrastructure.Redis.Extensions;
using MyPlatform.Services.Worker.Jobs;
using MyPlatform.Services.Worker.Services;
using MyPlatform.Services.Worker.Consumers;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

// ============================================================================
// 注册底座 SDK 服务
// ============================================================================

// 核心服务
builder.Services.AddPlatformCore(builder.Configuration);

// 多租户支持
builder.Services.AddPlatformMultiTenancy();

// Redis 连接
builder.Services.AddPlatformRedis(builder.Configuration);

// 多级缓存
builder.Services.AddPlatformCaching(builder.Configuration);

// 事件总线（RabbitMQ）
builder.Services.AddPlatformEventBus(builder.Configuration);

// 可观测性（日志、追踪、指标）
builder.Services.AddPlatformObservability(builder.Configuration);

// Outbox 配置
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.SectionName));

// ============================================================================
// 配置 Quartz.NET 调度器
// ============================================================================

builder.Services.AddPlatformScheduler(builder.Configuration, q =>
{
    // 配置 SampleCleanupJob - 每5分钟执行一次
    var cleanupJobKey = new JobKey("sample-cleanup-job", "sample-group");
    q.AddJob<SampleCleanupJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanupJobKey)
        .WithIdentity("sample-cleanup-trigger", "sample-group")
        .WithCronSchedule("0 */5 * * * ?")); // 每5分钟执行

    // 配置 SampleTenantJob - 每10分钟执行一次
    var tenantJobKey = new JobKey("sample-tenant-job", "sample-group");
    q.AddJob<SampleTenantJob>(opts => opts.WithIdentity(tenantJobKey));
    q.AddTrigger(opts => opts
        .ForJob(tenantJobKey)
        .WithIdentity("sample-tenant-trigger", "sample-group")
        .WithCronSchedule("0 */10 * * * ?")); // 每10分钟执行
});

// ============================================================================
// 注册后台服务
// ============================================================================

// Outbox 处理器服务
builder.Services.AddHostedService<OutboxProcessorService>();

// 事件消费者服务
builder.Services.AddHostedService<SampleEventConsumer>();

// ============================================================================
// 构建并启动主机
// ============================================================================

var host = builder.Build();

// 记录启动日志
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var environment = builder.Environment.EnvironmentName;
logger.LogInformation("=================================================");
logger.LogInformation("MyPlatform Worker 服务启动");
logger.LogInformation("环境: {Environment}", environment);
logger.LogInformation("启动时间: {StartTime:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
logger.LogInformation("=================================================");

await host.RunAsync();
