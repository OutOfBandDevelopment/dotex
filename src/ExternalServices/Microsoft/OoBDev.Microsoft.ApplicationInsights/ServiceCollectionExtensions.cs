using OoBDev.Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Microsoft.ApplicationInsights;

/// <summary>
/// Extension methods for configuring Application Insights services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add custom Application Insights telemetry processors to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the processors to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddApplicationInsightsExtensions(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryProcessor<CorrelationInfoTelemetryProcessor>();
        services.AddApplicationInsightsTelemetryProcessor<UserTelemetryProcessor>();
        return services;
    }
}
