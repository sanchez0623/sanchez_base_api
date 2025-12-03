using System.Diagnostics;

namespace MyPlatform.Gateway.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with timing information.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log request and response information.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? requestId;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";

        _logger.LogInformation(
            "Request started: {Method} {Path} | ClientIP: {ClientIp} | UserAgent: {UserAgent} | RequestId: {RequestId} | CorrelationId: {CorrelationId}",
            method, path, clientIp, userAgent, requestId, correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Request completed: {Method} {Path} | StatusCode: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId} | CorrelationId: {CorrelationId}",
                    method, path, statusCode, duration, requestId, correlationId);
            }
            else
            {
                _logger.LogInformation(
                    "Request completed: {Method} {Path} | StatusCode: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId} | CorrelationId: {CorrelationId}",
                    method, path, statusCode, duration, requestId, correlationId);
            }
        }
    }
}

/// <summary>
/// Extension methods for adding request logging middleware to the application pipeline.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds the request logging middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
