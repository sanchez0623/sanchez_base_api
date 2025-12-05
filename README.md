# Sanchez Base API

一个基于 .NET 8.0 LTS 的企业级微服务底座平台解决方案

## 项目简介

本项目采用领域驱动设计（DDD）架构，以 NuGet 包的形式为上层业务系统提供基础设施支撑。设计目标支持 10万+ QPS，可处理亿级数据规模。

## 项目结构

### 共享层（Shared Layer）

**Kernel 内核层**
- Entity、AggregateRoot、ValueObject 基础类型
- 领域事件机制
- 仓储接口定义
- 规约模式实现

**Contracts 契约层**
- 基础 DTO 定义
- 集成事件契约
- ApiResponse/PagedResponse 统一响应封装

**Utils 工具层**
- 雪花算法 ID 生成器
- JSON 序列化辅助工具
- 字符串/日期时间扩展方法

### SDK 层（NuGet 包）

| 模块 | 功能说明 |
|------|----------|
| **Core** | 全局配置管理、服务注册入口点 |
| **Authentication** | JWT Token 服务、密码哈希、当前用户上下文 |
| **Authorization** | RBAC 权限控制、动态策略提供器、权限处理器 |
| **MultiTenancy** | 租户上下文、基于请求头的租户解析器 |
| **Saga** | 编排模式实现、补偿机制、状态持久化、重试/超时策略 |
| **Idempotency** | X-Idempotency-Key 过滤器、Redis 去重、结果缓存 |
| **Scheduler** | Quartz.NET 集成、租户感知的定时任务 |
| **EventBus** | RabbitMQ 发布/订阅、Outbox 模式保证可靠性 |
| **Caching** | 多级缓存（本地 MemoryCache + Redis） |
| **ServiceCommunication** | HTTP 客户端工厂、Polly 重试/熔断策略 |
| **Observability** | Serilog 结构化日志、OpenTelemetry 链路追踪、Prometheus 指标 |
| **Search.Elasticsearch** | Elasticsearch SDK、查询构建器、索引管理、健康检查 |

### 基础设施层（Infrastructure）

**EFCore**
- 泛型仓储实现
- 工作单元模式
- 审计/软删除/租户拦截器

**Redis**
- 缓存服务
- 基于 RedLock 算法的分布式锁

### 示例服务与网关

- Identity 身份认证服务
- Order 订单服务（含 Saga 示例）
- YARP 反向代理网关

## 快速开始

### 基础服务注册

```csharp
builder.Services
    .AddPlatformCore(builder.Configuration)
    .AddPlatformAuthentication(builder.Configuration)
    .AddPlatformAuthorization()
    .AddPlatformMultiTenancy()
    .AddPlatformObservability(builder.Configuration);

builder.Services.AddPlatformEfCore<AppDbContext>((sp, options) =>
    options.UseNpgsql(connectionString));
```

### Elasticsearch SDK 使用

**配置文件**

```json
{
  "Elasticsearch": {
    "Nodes": ["http://localhost:9200"],
    "DefaultIndex": "my_index",
    "NumberOfShards": 3,
    "NumberOfReplicas": 1,
    "RequestTimeout": "00:00:30",
    "EnableDebugMode": false
  }
}
```

**服务注册**

```csharp
builder.Services.AddPlatformElasticsearch(builder.Configuration, options =>
{
    options.DefaultIndex = "my_index";
});

// 注册特定文档类型的搜索服务
builder.Services.AddSearchService<OrderSearchDocument>("order_search");

// 添加健康检查
builder.Services.AddElasticsearchHealthCheck();
```

**查询构建器示例**

```csharp
var query = SearchQueryBuilder<OrderSearchDocument>.Create()
    .WithTerm(d => d.Status, "paid")
    .WithMatch(d => d.CustomerName, "张三")
    .WithRange(d => d.TotalAmount, min: 100, max: 1000)
    .WithDateRange(d => d.CreatedAt, from: DateTime.Today.AddDays(-7))
    .WithFullText("iPhone", d => d.ProductNames, d => d.CustomerName)
    .WithNested("items", n => n.WithTerm("items.brandId", 10))
    .OrderByDescending(d => d.CreatedAt)
    .WithPaging(pageIndex: 1, pageSize: 20)
    .WithHighlight(d => d.CustomerName)
    .Build();

var results = await searchService.SearchAsync(query);
```

## 配置说明

| 文件 | 用途 |
|------|------|
| `Directory.Build.props` | 集中管理 NuGet 包版本 |
| `.editorconfig` | 代码风格配置 |
| `appsettings.json` | 各服务配置示例 |

## 技术栈

- .NET 8.0 LTS
- Entity Framework Core
- Redis
- RabbitMQ
- Elasticsearch
- Quartz.NET
- Serilog
- OpenTelemetry
- Polly
- YARP

## 开源协议

本项目基于 [Apache-2.0](LICENSE) 协议开源。
