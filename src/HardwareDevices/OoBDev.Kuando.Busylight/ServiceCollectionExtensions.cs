using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// Registration for the Busylight status light package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Busylight message encoder and decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddBusylight(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<BusylightIdentification>, BusylightMessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<BusylightCommand>, BusylightMessageEncoder>();
        return services;
    }
}
