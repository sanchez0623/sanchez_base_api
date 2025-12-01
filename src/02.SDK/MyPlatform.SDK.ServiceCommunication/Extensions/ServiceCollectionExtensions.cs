using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.ServiceCommunication.Configuration;
using MyPlatform.SDK.ServiceCommunication.Http;
using Polly;
using Polly.Extensions.Http;

namespace MyPlatform.SDK.ServiceCommunication.Extensions;

/// <summary>
/// Extension methods for registering ServiceCommunication services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a resilient HTTP client for a service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddResilientHttpClient(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration)
    {
        var httpOptions = configuration.GetSection("HttpClient").Get<HttpClientOptions>() ?? new HttpClientOptions();
        var endpoints = configuration.GetSection("Services").Get<Dictionary<string, ServiceEndpoint>>() ?? [];

        endpoints.TryGetValue(serviceName, out var endpoint);

        services.AddHttpClient<IServiceHttpClient, ServiceHttpClient>(serviceName, client =>
        {
            if (endpoint is not null)
            {
                client.BaseAddress = new Uri(endpoint.BaseUrl);

                foreach (var header in endpoint.Headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (!string.IsNullOrEmpty(endpoint.ApiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", endpoint.ApiKey);
                }
            }

            client.Timeout = TimeSpan.FromSeconds(httpOptions.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy(httpOptions))
        .AddPolicyHandler(GetCircuitBreakerPolicy(httpOptions));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpClientOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(options.RetryCount, retryAttempt =>
                TimeSpan.FromMilliseconds(options.RetrySleepDurationMs * Math.Pow(2, retryAttempt - 1)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(HttpClientOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                options.CircuitBreakerExceptionsAllowed,
                TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds));
    }
}
