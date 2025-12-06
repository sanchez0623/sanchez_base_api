using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Authorization.Extensions;
using MyPlatform.SDK.Authorization.Services;
using MyPlatform.SDK.EventBus.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using MyPlatform.Services.Messaging.Application.Services;
using MyPlatform.Services.Messaging.Infrastructure;
using MyPlatform.Services.Messaging.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// 基础服务
// =============================================================================
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddScoped<IPermissionChecker, DefaultPermissionChecker>(); // 注册权限检查器
builder.Services.AddPlatformAuthorization();
builder.Services.AddPlatformObservability(builder.Configuration);
builder.Services.AddPlatformMultiTenancy(builder.Configuration);

// =============================================================================
// 数据库配置
// =============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured.");

builder.Services.AddDbContext<MessagingDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// =============================================================================
// 消息队列（RabbitMQ/Kafka）
// =============================================================================
builder.Services.AddPlatformEventBus(builder.Configuration);

// =============================================================================
// 应用服务
// =============================================================================
builder.Services.AddScoped<EventPublishAppService>();

// =============================================================================
// Web API配置
// =============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Messaging Service API",
        Version = "v1",
        Description = "消息服务API，负责订单事件、库存同步、物流状态更新"
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MessagingDbContext>();

var app = builder.Build();

// =============================================================================
// 中间件管道
// =============================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging Service API v1");
    });
}

app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
