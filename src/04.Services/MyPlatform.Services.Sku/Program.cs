using Microsoft.EntityFrameworkCore;
using MyPlatform.Infrastructure.EFCore.Extensions;
using MyPlatform.Infrastructure.EFCore.ReadWriteSplit;
using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Authorization.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using MyPlatform.Services.Sku.Application.Services;
using MyPlatform.Services.Sku.Domain.Entities;
using MyPlatform.Services.Sku.Infrastructure.Data;
using MyPlatform.Services.Sku.Infrastructure.MultiTenancy;
using MyPlatform.Services.Sku.Infrastructure.Repositories;
using MyPlatform.Services.Sku.Infrastructure.Sharding;

var builder = WebApplication.CreateBuilder(args);

// 基础服务
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddPlatformAuthorization();
builder.Services.AddPlatformObservability(builder.Configuration);

// =============================================================================
// 多租户配置示例
// =============================================================================
// 选项 1：使用配置文件存储租户信息（仅用于开发/测试环境）
// 默认行为，在 appsettings.json 中配置 MultiTenancy:Tenants
builder.Services.AddPlatformMultiTenancy(builder.Configuration);

// 选项 2：使用 MySQL 数据库存储租户信息（生产环境推荐）
// 取消注释以下代码以启用数据库存储：
//
// var tenantConnectionString = builder.Configuration["MultiTenancy:TenantManagementConnectionString"];
// if (!string.IsNullOrEmpty(tenantConnectionString))
// {
//     builder.Services.AddDbContext<TenantManagementDbContext>(options =>
//         options.UseMySql(tenantConnectionString, ServerVersion.AutoDetect(tenantConnectionString)));
//     
//     builder.Services.AddTenantStore<MySqlTenantStore>();
// }
// =============================================================================

// 数据库配置（读写分离 + 分库分表）
builder.Services.AddPlatformEfCore<SkuDbContext>((sp, options) =>
{
    var connectionResolver = sp.GetService<IConnectionStringResolver>();
    var connectionString = connectionResolver?.GetWriteConnectionString()
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Database connection string is not configured.");

    // 使用 Pomelo MySQL 提供程序
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 添加读写分离支持
builder.Services.AddReadWriteSplit(builder.Configuration);

// 添加分库分表支持
builder.Services.AddSharding<SkuDbContext>(builder.Configuration);

// 注册分片路由规则
builder.Services.AddShardingRoute<ProductSku, SkuModShardingRoute>();
builder.Services.AddShardingRoute<SkuStock, StockModShardingRoute>();
builder.Services.AddShardingRoute<SkuPriceHistory, PriceHistoryTimeShardingRoute>();

// 注册仓储
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISkuRepository, SkuRepository>();

// 注册应用服务
builder.Services.AddScoped<ProductAppService>();
builder.Services.AddScoped<SkuAppService>();
builder.Services.AddScoped<StockAppService>();

// 添加控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SKU Management API",
        Version = "v1",
        Description = "SKU管理示例API，展示读写分离和分库分表功能"
    });
});

// 添加健康检查
builder.Services.AddHealthChecks();

var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SKU Management API v1");
    });
}

app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
