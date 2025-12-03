using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyPlatform.Gateway.Extensions;
using MyPlatform.Gateway.Middleware;
using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Observability.Extensions;
using System.Text.Json;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Service Registration
// ============================================

// Add Platform SDK services
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddPlatformObservability(builder.Configuration);

// Add Gateway-specific services
builder.Services.AddGatewayRateLimiting(builder.Configuration);
builder.Services.AddGatewayCors(builder.Configuration, builder.Environment);
builder.Services.AddGatewayCompression();
builder.Services.AddGatewayHealthChecks();

// Add HTTP client factory for health checks
builder.Services.AddHttpClient();

// Add authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Add YARP reverse proxy with transforms
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms =>
    {
        // Request transforms - add tracing and user info headers
        transforms.AddRequestTransform(context =>
        {
            var httpContext = context.HttpContext;

            // Add correlation ID
            var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? httpContext.TraceIdentifier;
            context.ProxyRequest.Headers.Remove("X-Correlation-Id");
            context.ProxyRequest.Headers.Add("X-Correlation-Id", correlationId);

            // Add gateway identifier
            context.ProxyRequest.Headers.Remove("X-Forwarded-Gateway");
            context.ProxyRequest.Headers.Add("X-Forwarded-Gateway", "MyPlatform.Gateway");

            // Add user information if authenticated
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("UserId")?.Value;
                var tenantId = httpContext.User.FindFirst("TenantId")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    context.ProxyRequest.Headers.Remove("X-User-Id");
                    context.ProxyRequest.Headers.Add("X-User-Id", userId);
                }

                if (!string.IsNullOrEmpty(tenantId))
                {
                    context.ProxyRequest.Headers.Remove("X-Tenant-Id");
                    context.ProxyRequest.Headers.Add("X-Tenant-Id", tenantId);
                }
            }

            return ValueTask.CompletedTask;
        });

        // Response transforms - remove sensitive headers and add gateway identifier
        transforms.AddResponseTransform(context =>
        {
            if (context.ProxyResponse != null)
            {
                // Remove sensitive headers
                context.HttpContext.Response.Headers.Remove("Server");
                context.HttpContext.Response.Headers.Remove("X-Powered-By");

                // Add gateway identifier
                context.HttpContext.Response.Headers["X-Gateway"] = "MyPlatform.Gateway";
            }

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// ============================================
// Middleware Pipeline
// ============================================

// 1. Exception handling (first in pipeline)
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "InternalServerError",
            message = "An unexpected error occurred. Please try again later.",
            correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

// 2. Response compression
app.UseResponseCompression();

// 3. Correlation ID (early to ensure all logs have it)
app.UseCorrelationId();

// 4. Request logging
app.UseRequestLogging();

// 5. CORS
app.UseCors("GatewayPolicy");

// 6. Rate limiting
app.UseRateLimiter();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthCheckResponse
});

// 9. YARP reverse proxy
app.MapReverseProxy();

app.Run();

// Writes a JSON response for health check endpoints.
static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            duration = e.Value.Duration.TotalMilliseconds,
            description = e.Value.Description,
            data = e.Value.Data
        })
    };

    return context.Response.WriteAsJsonAsync(response, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    });
}
