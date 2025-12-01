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
    
Configuration
    Directory.Build.props: Centralized package version management
    .editorconfig: Code style configuration
    Sample appsettings.json for each service
