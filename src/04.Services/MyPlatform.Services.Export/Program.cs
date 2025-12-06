using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Authorization.Extensions;
using MyPlatform.SDK.DataExchange.Jobs;
using MyPlatform.SDK.EventBus.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using MyPlatform.SDK.Storage.Extensions;
using MyPlatform.Services.Export.Application.Services;
using MyPlatform.Services.Export.Infrastructure.Data;
using MyPlatform.Services.Export.Infrastructure.Repositories;
using MyPlatform.Services.Export.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// 基础服务
// =============================================================================
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddPlatformAuthorization();
builder.Services.AddPlatformObservability(builder.Configuration);
builder.Services.AddPlatformMultiTenancy(builder.Configuration);

// =============================================================================
// 数据库配置
// =============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured.");

builder.Services.AddDbContext<ExportDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// =============================================================================
// 存储服务（OSS上传）
// =============================================================================
builder.Services.AddPlatformStorage(builder.Configuration);

// =============================================================================
// 消息队列（作业分发）
// =============================================================================
builder.Services.AddPlatformEventBus(builder.Configuration);

// =============================================================================
// 仓储和应用服务
// =============================================================================
builder.Services.AddScoped<IDataExchangeJobRepository, EfExportJobRepository>();
builder.Services.AddScoped<ExportAppService>();
builder.Services.AddScoped<ExportJobWorker>();

// =============================================================================
// Web API配置
// =============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Export Service API",
        Version = "v1",
        Description = "数据导出微服务API，支持Excel/CSV导出、大文件异步处理"
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ExportDbContext>();

var app = builder.Build();

// =============================================================================
// 中间件管道
// =============================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Export Service API v1");
    });
}

app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
