using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Scpi;

/// <summary>
/// Registration for the SCPI package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SCPI message decoder and encoder and the profile catalog (bundled profiles only; use
    /// <see cref="ScpiProfileCatalog.Create"/> for drop-in directories).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddScpi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<ScpiReply>, ScpiMessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<ScpiRequest>, ScpiMessageEncoder>();
        services.TryAddSingleton(_ => ScpiProfileCatalog.Create());
        return services;
    }
}
