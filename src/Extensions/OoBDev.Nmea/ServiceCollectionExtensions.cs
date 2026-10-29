using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Nmea;

/// <summary>
/// Registration for the NMEA package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the NMEA message decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddNmea(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<NmeaSentence>, NmeaMessageDecoder>();
        return services;
    }
}
