using OoBDev.Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace OoBDev.Microsoft.ApplicationInsights;

/// <summary>
/// Extension methods for configuring Application Insights services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add the custom OpenTelemetry processors (correlation and user information for spans and logs)
    /// that Application Insights 3.x uses in place of telemetry processors.
    /// </summary>
    /// <param name="services">The service collection to add the processors to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddApplicationInsightsExtensions(this IServiceCollection services)
    {
        services.TryAddSingleton<CorrelationInfoTelemetryProcessor>();
        services.TryAddSingleton<UserTelemetryProcessor>();
        services.TryAddSingleton<CorrelationInfoLogProcessor>();
        services.TryAddSingleton<UserLogProcessor>();

        services.ConfigureOpenTelemetryTracerProvider((sp, builder) => builder
            .AddProcessor(sp.GetRequiredService<CorrelationInfoTelemetryProcessor>())
            .AddProcessor(sp.GetRequiredService<UserTelemetryProcessor>()));
        services.ConfigureOpenTelemetryLoggerProvider((sp, builder) => builder
            .AddProcessor(sp.GetRequiredService<CorrelationInfoLogProcessor>())
            .AddProcessor(sp.GetRequiredService<UserLogProcessor>()));
        return services;
    }
}
