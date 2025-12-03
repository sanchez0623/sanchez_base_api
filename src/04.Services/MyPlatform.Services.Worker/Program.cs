using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.Caching.Extensions;
using MyPlatform.SDK.Core.Extensions;
using MyPlatform.SDK.EventBus.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using MyPlatform.SDK.Scheduler.Extensions;
using MyPlatform.SDK.Scheduler.Configuration;
using MyPlatform.Infrastructure.Redis.Extensions;
using MyPlatform.Services.Worker.Configuration;
using MyPlatform.Services.Worker.Data;
using MyPlatform.Services.Worker.Jobs;
using MyPlatform.Services.Worker.Listeners;
using MyPlatform.Services.Worker.Services;
using MyPlatform.Services.Worker.Consumers;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

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
// 配置数据库上下文（用于任务执行历史）
// ============================================================================

var workerConnectionString = builder.Configuration.GetConnectionString("WorkerConnection");
if (!string.IsNullOrEmpty(workerConnectionString))
{
    builder.Services.AddDbContext<WorkerDbContext>(options =>
        options.UseMySql(
            workerConnectionString,
            ServerVersion.AutoDetect(workerConnectionString),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure()));
}
else
{
    // 当没有配置数据库连接时，仍然注册 DbContext 但使用 MySQL 本地数据库
    // 这样可以在开发环境中测试，或者在生产环境中必须配置数据库
    builder.Services.AddDbContext<WorkerDbContext>(options =>
        options.UseMySql(
            "Server=localhost;Port=3306;Database=worker;Uid=root;Pwd=root;",
            new MySqlServerVersion(new Version(8, 0, 0)),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure()));
}

// ============================================================================
// 配置任务调度选项
// ============================================================================

builder.Services.Configure<JobScheduleOptions>(
    builder.Configuration.GetSection(JobScheduleOptions.SectionName));

// ============================================================================
// 注册任务管理服务
// ============================================================================

builder.Services.AddScoped<IJobManagementService, JobManagementService>();
builder.Services.AddScoped<IJobExecutionHistoryService, JobExecutionHistoryService>();
builder.Services.AddSingleton<JobExecutionHistoryListener>();

// ============================================================================
// 配置 Quartz.NET 调度器
// ============================================================================

var schedulerOptions = builder.Configuration
    .GetSection("Scheduler")
    .Get<SchedulerOptions>() ?? new SchedulerOptions();

var jobScheduleOptions = builder.Configuration
    .GetSection(JobScheduleOptions.SectionName)
    .Get<JobScheduleOptions>() ?? new JobScheduleOptions();

builder.Services.AddQuartz(q =>
{
    q.SchedulerName = schedulerOptions.InstanceName;

    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = schedulerOptions.ThreadCount;
    });

    // 配置持久化存储（如果启用）
    var quartzConnectionString = builder.Configuration.GetConnectionString("QuartzConnection");
    if (schedulerOptions.UsePersistentStore && !string.IsNullOrEmpty(quartzConnectionString))
    {
        q.UsePersistentStore(store =>
        {
            store.UseProperties = true;
            store.UseMySql(quartzConnectionString);
            store.UseClustering();
#pragma warning disable CS0618 // UseJsonSerializer is deprecated but works for our use case
            store.UseJsonSerializer();
#pragma warning restore CS0618
        });
    }

    // 注册任务执行历史监听器
    q.AddJobListener<JobExecutionHistoryListener>();

    // 从配置文件注册任务
    foreach (var (jobName, jobConfig) in jobScheduleOptions.Jobs)
    {
        if (!jobConfig.Enabled)
        {
            continue;
        }

        // 尝试获取任务类型
        Type? jobType = null;
        if (!string.IsNullOrEmpty(jobConfig.JobType))
        {
            jobType = Type.GetType(jobConfig.JobType);
        }

        // 如果无法从配置获取类型，尝试匹配已知任务
        if (jobType == null)
        {
            jobType = jobName switch
            {
                "SampleCleanupJob" => typeof(SampleCleanupJob),
                "SampleTenantJob" => typeof(SampleTenantJob),
                _ => null
            };
        }

        if (jobType == null || !typeof(IJob).IsAssignableFrom(jobType))
        {
            continue;
        }

        var jobKey = new JobKey(jobName, jobConfig.Group);
        var triggerKey = new TriggerKey($"{jobName}-trigger", jobConfig.Group);

        q.AddJob(jobType, jobKey, opts =>
        {
            opts.WithDescription(jobConfig.Description);
            opts.StoreDurably();

            if (jobConfig.JobData != null)
            {
                foreach (var (key, value) in jobConfig.JobData)
                {
                    opts.UsingJobData(key, value);
                }
            }
        });

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity(triggerKey)
            .WithDescription(jobConfig.Description)
            .WithCronSchedule(jobConfig.CronExpression));
    }
});

builder.Services.AddQuartzHostedService(opts =>
{
    opts.WaitForJobsToComplete = schedulerOptions.WaitForJobsToComplete;
});

// ============================================================================
// 配置 Web API（控制器、Swagger）
// ============================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MyPlatform Worker API",
        Version = "v1",
        Description = "任务管理 API - 提供动态任务管理、执行历史查询等功能"
    });

    // 添加 XML 注释
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ============================================================================
// 注册后台服务
// ============================================================================

// Outbox 处理器服务
builder.Services.AddHostedService<OutboxProcessorService>();

// 事件消费者服务
builder.Services.AddHostedService<SampleEventConsumer>();

// ============================================================================
// 构建应用
// ============================================================================

var app = builder.Build();

// 记录启动日志
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var environment = builder.Environment.EnvironmentName;
logger.LogInformation("=================================================");
logger.LogInformation("MyPlatform Worker 服务启动");
logger.LogInformation("环境: {Environment}", environment);
logger.LogInformation("启动时间: {StartTime:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
logger.LogInformation("API 端点: http://localhost:5100");
logger.LogInformation("Swagger UI: http://localhost:5100/swagger");
logger.LogInformation("=================================================");

// ============================================================================
// 配置中间件管道
// ============================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPlatform Worker API v1");
    });
}

app.UseRouting();
app.MapControllers();

// 健康检查端点
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// ============================================================================
// 初始化数据库（确保数据库已创建）
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Worker 数据库初始化完成");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Worker 数据库初始化失败，将使用内存存储");
    }
}

await app.RunAsync();
