using OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;
using OoBDev.AspNetCore.Mvc.Middleware;
using Microsoft.AspNetCore.Builder;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// This is a set of extension configurations for ASP.Net Core
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Add custom middleware to ASP.Net to support these extensions
    /// <see cref="CorrelationInfoMiddleware"/>
    /// <see cref="CultureInfoMiddleware"/> 
    /// <see cref="SearchQueryMiddleware"/> 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="healthCheckPath"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAspNetCoreExtensionMiddleware(
        this IApplicationBuilder builder,
#if DEBUG
        string? healthCheckPath
#else 
        string? healthCheckPath = "/health"
#endif

        )
    {
        healthCheckPath ??= "/health";
        builder.UseHealthChecks(healthCheckPath, HealthCheckOptionsFactory.Create());
        builder.UseMiddleware<CorrelationInfoMiddleware>();
        builder.UseMiddleware<CultureInfoMiddleware>();
        builder.UseMiddleware<SearchQueryMiddleware>();
        return builder;
    }
}
