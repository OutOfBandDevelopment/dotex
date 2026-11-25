using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// Registration for the Zoom H4n package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Zoom H4n message encoder and decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddZoomH4n(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<ZoomH4nStatus>, ZoomH4nMessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<ZoomH4nCommand>, ZoomH4nMessageEncoder>();
        return services;
    }
}
