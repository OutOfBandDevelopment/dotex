using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.ZWave;

/// <summary>
/// Registration for the Z-Wave Serial API package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Z-Wave message decoder and encoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddZWave(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<ZWaveFrame>, ZWaveMessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<ZWaveFrame>, ZWaveMessageEncoder>();
        return services;
    }
}
