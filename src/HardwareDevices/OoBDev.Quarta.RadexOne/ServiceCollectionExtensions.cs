using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// Registration for the Quarta Radex One package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Radex One message encoder and decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddRadexOne(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<RadexOneReply>, RadexOneMessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<RadexOneRequest>, RadexOneMessageEncoder>();
        return services;
    }
}
