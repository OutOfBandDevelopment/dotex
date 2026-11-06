using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>
/// Extension methods for configuring communication services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add communication-related services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddCommunicationsServices(this IServiceCollection services) => services;

    /// <summary>
    /// Adds <see cref="ISendService"/> and an in-memory <see cref="IDeliveryTracker"/>. The caller registers
    /// <see cref="IChannelRepository"/>, <see cref="IChannelRegistry"/> and the providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings override.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddSendReceive(this IServiceCollection services, Action<SendReceiveOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<SendReceiveOptions>().BindConfiguration("OoBDev:Communications");
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IDeliveryTracker, InMemoryDeliveryTracker>();
        services.TryAddSingleton<ISendService, SendService>();
        return services;
    }
}
