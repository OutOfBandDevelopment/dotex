using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// Registration for the Velleman K8055 package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the K8055 message encoder and decoder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddK8055(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageDecoder<K8055Reading>, K8055MessageDecoder>();
        services.TryAddSingleton<IMessageEncoder<K8055Command>, K8055MessageEncoder>();
        return services;
    }
}
