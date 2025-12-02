# sanchez_base_api
一个基于.net core的企业级微服务底座平台解决方案

Enterprise-level microservices foundation platform for .NET 8.0 LTS with DDD architecture, designed as NuGet packages for upper-layer business systems. Supports 100k+ QPS with billion-scale data targets.

Shared Layer

    Kernel: Entity, AggregateRoot, ValueObject, domain events, repository interfaces, specifications
    Contracts: Base DTOs, integration events, ApiResponse/PagedResponse wrappers
    Utils: Snowflake ID generator, JSON helpers, string/datetime extensions

SDK Layer (NuGet Packages)

    Core: Global configuration, service registration entry point
    Authentication: JWT token service, password hashing, current user context
    Authorization: RBAC with dynamic policy provider, permission handlers
    MultiTenancy: Tenant context, header-based resolver
    Saga: Orchestration pattern with compensation, state persistence, retry/timeout
    Idempotency: X-Idempotency-Key filter, Redis deduplication, result caching
    Scheduler: Quartz.NET integration with tenant-aware jobs
    EventBus: RabbitMQ publisher/subscriber, Outbox pattern for reliability
    Caching: Multi-level cache (local MemoryCache + Redis)
    ServiceCommunication: HTTP client factory with Polly retry/circuit breaker
    Observability: Serilog structured logging, OpenTelemetry tracing, Prometheus metrics
    Search.Elasticsearch: Elasticsearch SDK with query builder, index management, and health checks

Infrastructure

    EFCore: Generic repository, unit of work, auditable/soft-delete/tenant interceptors
    Redis: Cache service, distributed lock with RedLock algorithm
    Sample Services & Gateway
    Identity service and Order service (with Saga example)
    YARP reverse proxy gateway
    
Usage Example

    builder.Services
        .AddPlatformCore(builder.Configuration)
        .AddPlatformAuthentication(builder.Configuration)
        .AddPlatformAuthorization()
        .AddPlatformMultiTenancy()
        .AddPlatformObservability(builder.Configuration);

    builder.Services.AddPlatformEfCore<AppDbContext>((sp, options) =>
        options.UseNpgsql(connectionString));

Search.Elasticsearch SDK

The Search.Elasticsearch SDK provides a comprehensive Elasticsearch integration for microservices.

Features:
- Generic search document base class
- Fluent query builder with chainable API
- Index management service
- Health checks for ES cluster
- Event-driven index sync handlers

Configuration:

    // appsettings.json
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

Registration:

    builder.Services.AddPlatformElasticsearch(builder.Configuration, options =>
    {
        options.DefaultIndex = "my_index";
    });
    
    // Register search service for a specific document type
    builder.Services.AddSearchService<OrderSearchDocument>("order_search");
    
    // Add health check
    builder.Services.AddElasticsearchHealthCheck();

Query Builder Example:

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
    
Configuration
    Directory.Build.props: Centralized package version management
    .editorconfig: Code style configuration
    Sample appsettings.json for each service
