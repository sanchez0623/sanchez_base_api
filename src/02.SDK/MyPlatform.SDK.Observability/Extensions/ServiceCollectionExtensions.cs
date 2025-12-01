using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyPlatform.SDK.Observability.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace MyPlatform.SDK.Observability.Extensions;

/// <summary>
/// Extension methods for registering Observability services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds observability services (logging, tracing, metrics) to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Observability").Get<ObservabilityOptions>() ?? new ObservabilityOptions();
        services.Configure<ObservabilityOptions>(configuration.GetSection("Observability"));

        // Add OpenTelemetry tracing
        if (options.Tracing.Enabled)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: options.ServiceName,
                        serviceVersion: options.ServiceVersion,
                        serviceInstanceId: Environment.MachineName))
                .WithTracing(tracing =>
                {
                    if (options.Tracing.TraceAspNetCore)
                    {
                        tracing.AddAspNetCoreInstrumentation();
                    }

                    if (options.Tracing.TraceHttpClient)
                    {
                        tracing.AddHttpClientInstrumentation();
                    }

                    if (options.EnableConsoleExporter)
                    {
                        tracing.AddConsoleExporter();
                    }
                });
        }

        // Add OpenTelemetry metrics
        if (options.Metrics.Enabled)
        {
            services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation();
                    metrics.AddHttpClientInstrumentation();

                    if (options.Metrics.ExposePrometheus)
                    {
                        metrics.AddPrometheusExporter();
                    }

                    if (options.EnableConsoleExporter)
                    {
                        metrics.AddConsoleExporter();
                    }
                });
        }

        return services;
    }

    /// <summary>
    /// Configures Serilog logging for the host builder.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UsePlatformSerilog(this IHostBuilder builder, IConfiguration configuration)
    {
        var options = configuration.GetSection("Observability:Logging").Get<LoggingOptions>() ?? new LoggingOptions();

        return builder.UseSerilog((context, services, loggerConfig) =>
        {
            var minLevel = Enum.Parse<LogEventLevel>(options.MinimumLevel, ignoreCase: true);

            loggerConfig
                .MinimumLevel.Is(minLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName();

            if (options.WriteToConsole)
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            if (options.WriteToFile && !string.IsNullOrEmpty(options.FilePath))
            {
                var rollingInterval = Enum.Parse<Serilog.RollingInterval>(options.RollingInterval, ignoreCase: true);
                loggerConfig.WriteTo.File(
                    path: options.FilePath,
                    rollingInterval: rollingInterval,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    retainedFileCountLimit: options.RetainedFileCountLimit);
            }
        });
    }
}

/// <summary>
/// Extension methods for configuring observability middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Prometheus metrics endpoint.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="path">The endpoint path.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app, string path = "/metrics")
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint(path);
        return app;
    }
}
