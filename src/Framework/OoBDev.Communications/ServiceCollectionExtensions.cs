using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>
/// Extension methods for configuring communication services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ISendService"/> and an in-memory <see cref="IDeliveryTracker"/>. The caller registers
    /// the <see cref="IChannelProvider"/> implementations (channels come from configuration unless <see cref="IChannelRepository"/> is replaced).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings override.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddSendReceive(this IServiceCollection services, Action<SendReceiveOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<SendReceiveOptions>().BindConfiguration("OoBDev:Communications").ValidateDataAnnotations().ValidateOnStart();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IDeliveryTracker, InMemoryDeliveryTracker>();
        services.TryAddSingleton<IChannelRepository, ConfigurationChannelRepository>();
        services.TryAddSingleton<IChannelRegistry, ChannelRegistry>();
        services.TryAddSingleton<ISendService, SendService>();
        services.TryAddSingleton<IWebhookProcessorFactory, WebhookProcessorFactory>();
        services.TryAddSingleton<IReceiveService, ReceiveService>();
        return services;
    }
}
