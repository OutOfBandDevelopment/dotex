using OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;
using OoBDev.AspNetCore.Mvc.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// This is a set of extension configurations for ASP.Net Core
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Accepts the HTTP <c>QUERY</c> verb and its <c>POST</c> fallback on <c>GET</c> endpoints. Call before routing.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseHttpQueryVerb(this IApplicationBuilder builder)
        => builder.UseMiddleware<QueryVerbMiddleware>();

    /// <summary>
    /// Returns unhandled exceptions as RFC 9457 problem details with trace and correlation identifiers.
    /// Call first, before <see cref="UseAspNetCoreExtensionMiddleware"/>, after <c>TryAddProblemDetailsExceptionHandler</c>.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseProblemDetailsExceptionHandler(this IApplicationBuilder builder)
        => builder.UseExceptionHandler();

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
        var mapped = builder.ApplicationServices.GetService<HealthCheckPath>();
        if (mapped is not null)
            mapped.Value = healthCheckPath;
        builder.UseHealthChecks(healthCheckPath, HealthCheckOptionsFactory.Create());
        builder.UseMiddleware<CorrelationInfoMiddleware>();
        builder.UseMiddleware<CultureInfoMiddleware>();
        builder.UseMiddleware<SearchQueryMiddleware>();
        return builder;
    }
}
