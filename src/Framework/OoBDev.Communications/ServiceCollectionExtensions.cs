using System;
using System.Linq;
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

    /// <summary>
    /// Wraps the registered <see cref="ISendService"/> so suppressed addresses are removed before sending. Call it
    /// after <see cref="TryAddSendReceive"/>; register a durable <see cref="ISuppressionList"/> first to replace the in-memory one.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEmailSuppression(this IServiceCollection services)
    {
        var inner = services.LastOrDefault(d => d.ServiceType == typeof(ISendService))
            ?? throw new InvalidOperationException("Register ISendService (TryAddSendReceive) before AddEmailSuppression.");
        services.Remove(inner);
        services.TryAddSingleton<ISuppressionList, InMemorySuppressionList>();
        services.AddSingleton<ISendService>(provider =>
        {
            var real = inner.ImplementationFactory != null
                ? (ISendService)inner.ImplementationFactory(provider)
                : inner.ImplementationInstance != null
                    ? (ISendService)inner.ImplementationInstance
                    : (ISendService)ActivatorUtilities.CreateInstance(provider, inner.ImplementationType!);
            return new SuppressingSendService(real, provider.GetRequiredService<ISuppressionList>(), provider.GetRequiredService<TimeProvider>());
        });
        return services;
    }
}
