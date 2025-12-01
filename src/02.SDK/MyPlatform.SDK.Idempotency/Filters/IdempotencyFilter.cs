using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Idempotency.Configuration;
using MyPlatform.SDK.Idempotency.Services;

namespace MyPlatform.SDK.Idempotency.Filters;

/// <summary>
/// Action filter for handling idempotent requests.
/// </summary>
public class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly IdempotencyOptions _options;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(
        IIdempotencyService idempotencyService,
        IOptions<IdempotencyOptions> options,
        ILogger<IdempotencyFilter> logger)
    {
        _idempotencyService = idempotencyService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // Only apply to mutating methods
        if (!HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method))
        {
            await next();
            return;
        }

        // Get idempotency key from header
        var idempotencyKey = request.Headers[_options.HeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            if (_options.RequireIdempotencyKey)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Error = $"Missing required header: {_options.HeaderName}"
                });
                return;
            }

            await next();
            return;
        }

        _logger.LogDebug("Processing idempotent request with key: {IdempotencyKey}", idempotencyKey);

        var (shouldProceed, @lock, cachedResult) = await _idempotencyService.TryAcquireAsync(idempotencyKey);

        if (!shouldProceed)
        {
            if (cachedResult is not null)
            {
                _logger.LogDebug("Returning cached result for idempotency key: {IdempotencyKey}", idempotencyKey);

                context.HttpContext.Response.StatusCode = cachedResult.StatusCode;
                if (!string.IsNullOrEmpty(cachedResult.ContentType))
                {
                    context.HttpContext.Response.ContentType = cachedResult.ContentType;
                }
                if (!string.IsNullOrEmpty(cachedResult.Body))
                {
                    await context.HttpContext.Response.WriteAsync(cachedResult.Body);
                }

                context.Result = new EmptyResult();
                return;
            }

            context.Result = new StatusCodeResult(StatusCodes.Status409Conflict);
            return;
        }

        try
        {
            // Enable response buffering to capture the response body
            var originalBodyStream = context.HttpContext.Response.Body;
            using var responseBody = new MemoryStream();
            context.HttpContext.Response.Body = responseBody;

            var result = await next();

            // Capture and store the response
            if (_options.EnableResultCaching && result.Exception is null)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var bodyContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                var idempotentResult = new IdempotentResult
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Body = bodyContent,
                    ContentType = context.HttpContext.Response.ContentType
                };

                var expiry = TimeSpan.FromSeconds(_options.DefaultExpirationSeconds);
                await _idempotencyService.StoreResultAsync(idempotencyKey, idempotentResult, expiry);

                _logger.LogDebug("Stored result for idempotency key: {IdempotencyKey}", idempotencyKey);
            }

            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
            context.HttpContext.Response.Body = originalBodyStream;
        }
        finally
        {
            if (@lock is not null)
            {
                await @lock.ReleaseAsync();
            }
        }
    }
}
