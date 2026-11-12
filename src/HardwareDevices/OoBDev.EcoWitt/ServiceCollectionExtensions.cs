using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.EcoWitt;

/// <summary>
/// Registration for the EcoWitt package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EcoWitt push decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEcoWitt(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<EcoWittPushDecoder>();
        return services;
    }
}
