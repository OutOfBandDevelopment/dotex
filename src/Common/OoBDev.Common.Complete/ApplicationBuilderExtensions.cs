using OoBDev.Common.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace OoBDev.Common;

/// <summary>
/// Provides extension methods for configuring common middleware on the <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures all common middleware on the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <param name="middlewareBuilder">The <see cref="MiddlewareExtensionBuilder"/> instance.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseAllCommonMiddleware(
        this IApplicationBuilder builder,
#if DEBUG
        MiddlewareExtensionBuilder? middlewareBuilder
#else
        MiddlewareExtensionBuilder? middlewareBuilder = default
#endif
        )
    {
        middlewareBuilder ??= new();
        builder.UseCommonAspNetCoreMiddleware(middlewareBuilder);
        return builder;
    }
}
