using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MyPlatform.Gateway.HealthChecks;

namespace MyPlatform.Gateway.Extensions;

/// <summary>
/// Extension methods for registering Gateway services.
/// </summary>
public static class GatewayServiceExtensions
{
    /// <summary>
    /// Adds rate limiting services with global and route-specific policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter based on IP address
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = configuration.GetValue("RateLimiting:Global:PermitLimit", 100),
                        Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Global:WindowSeconds", 60))
                    });
            });

            // Custom 429 response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter).ToString();

                var response = new
                {
                    error = "TooManyRequests",
                    message = "Request rate limit exceeded. Please try again later.",
                    retryAfterSeconds = (int)retryAfter
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };

            // Route-specific rate limiting policies
            // API policy - for general API routes
            options.AddFixedWindowLimiter("api", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue("RateLimiting:Api:PermitLimit", 50);
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Api:WindowSeconds", 60));
                limiterOptions.AutoReplenishment = true;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = configuration.GetValue("RateLimiting:Api:QueueLimit", 10);
            });

            // Auth policy - stricter limits for authentication routes
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60));
                limiterOptions.AutoReplenishment = true;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = configuration.GetValue("RateLimiting:Auth:QueueLimit", 5);
            });

            // Sliding window policy for high-traffic endpoints
            options.AddSlidingWindowLimiter("sliding", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue("RateLimiting:Sliding:PermitLimit", 100);
                limiterOptions.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Sliding:WindowSeconds", 60));
                limiterOptions.SegmentsPerWindow = configuration.GetValue("RateLimiting:Sliding:SegmentsPerWindow", 6);
                limiterOptions.AutoReplenishment = true;
            });
        });

        return services;
    }

    /// <summary>
    /// Adds CORS policies for the Gateway with environment-aware configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The web host environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("GatewayPolicy", builder =>
            {
                if (environment.IsDevelopment())
                {
                    // Allow all origins in development
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("X-Correlation-Id", "X-Gateway");
                }
                else
                {
                    // Production: read allowed origins from configuration
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                        ?? Array.Empty<string>();

                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("X-Correlation-Id", "X-Gateway");
                }
            });

            // Strict policy for sensitive endpoints
            options.AddPolicy("StrictPolicy", builder =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? Array.Empty<string>();

                builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST")
                    .WithHeaders("Authorization", "Content-Type", "X-Correlation-Id")
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Adds health check services including downstream service health checks.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DownstreamServicesHealthCheck>(
                "downstream-services",
                tags: new[] { "ready" });

        return services;
    }

    /// <summary>
    /// Adds response compression services for the Gateway.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = new[]
            {
                "application/json",
                "application/xml",
                "text/plain",
                "text/json",
                "text/xml",
                "text/html"
            };
        });

        return services;
    }
}
