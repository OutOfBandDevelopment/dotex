using OoBDev.AspNetCore.JwtAuthentication;
using OoBDev.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Common.AspNetCore;

/// <summary>
/// Provides extension methods for configuring common ASP.NET Core extensions in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add common ASP.NET Core extensions to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">The configuration containing settings for ASP.NET Core extensions.</param>
    /// <param name="aspNetBuilder">Optional builder for configuring ASP.NET Core extensions. Default is <c>null</c>.</param>
    /// <param name="jwtBuilder">Optional builder for configuring JWT extensions. Default is <c>null</c>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection TryCommonAspNetCoreExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        AspNetCoreExtensionBuilder? aspNetBuilder,
        JwtExtensionBuilder? jwtBuilder
#else
        AspNetCoreExtensionBuilder? aspNetBuilder = default,
        JwtExtensionBuilder? jwtBuilder = default
#endif
    )
    {
        // Add common ASP.NET Core extensions
        services.TryAddAspNetCoreExtensions(aspNetBuilder);

        // Add JWT Bearer services
        services.TryAddJwtBearerServices(configuration, jwtBuilder);

        return services;
    }
}
