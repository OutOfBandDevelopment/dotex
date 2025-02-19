using OoBDev.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;

namespace OoBDev.Common.AspNetCore;

/// <summary>
/// Provides extension methods for configuring common ASP.NET Core middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds common ASP.NET Core middleware to the specified application builder.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <param name="middlewareBuilder">The <see cref="MiddlewareExtensionBuilder"/> instance.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseCommonAspNetCoreMiddleware(
        this IApplicationBuilder builder,
#if DEBUG
        MiddlewareExtensionBuilder? middlewareBuilder
#else
        MiddlewareExtensionBuilder? middlewareBuilder = default
#endif
        )
    {
        middlewareBuilder ??= new();
        builder.UseAspNetCoreExtensionMiddleware(
            healthCheckPath: middlewareBuilder.HealthCheckPath
        );
        return builder;
    }
}
