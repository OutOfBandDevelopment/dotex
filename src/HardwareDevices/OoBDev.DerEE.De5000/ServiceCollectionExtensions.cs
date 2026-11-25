using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// Registration for the DE-5000 LCR meter package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the DE-5000 message decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddDe5000(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<De5000Frame>, De5000MessageDecoder>();
        return services;
    }
}
