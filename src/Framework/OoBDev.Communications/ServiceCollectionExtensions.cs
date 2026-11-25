using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Orchestration;
using OoBDev.Communications.Unsubscribe;

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
    /// Adds <see cref="ICommunicationOrchestrator"/> with an in-memory <see cref="IDeferralStore"/> and <see cref="ITargetPreferenceProvider"/>.
    /// Register <see cref="IMessageComposer"/> and <see cref="IDataEnhancer"/> implementations (as enumerable services), and replace the store and provider with durable ones.
    /// Call <see cref="ICommunicationOrchestrator.ReleaseDueAsync"/> on a timer or a hosted service to release held requests.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddCommunicationOrchestration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IDeferralStore, InMemoryDeferralStore>();
        services.TryAddSingleton<ITargetPreferenceProvider, InMemoryTargetPreferenceProvider>();
        services.TryAddSingleton<ICommunicationOrchestrator, CommunicationOrchestrator>();
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
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDeliveryStatusHandler, SuppressionStatusHandler>());
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

    /// <summary>
    /// Adds <see cref="UnsubscribeLinks"/> (RFC 8058 one-click unsubscribe). Options bind from <c>OoBDev:Communications:Unsubscribe</c>.
    /// The caller maps an HTTPS POST (and GET) endpoint that calls <see cref="UnsubscribeLinks.UnsubscribeAsync"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings override.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddUnsubscribeLinks(this IServiceCollection services, Action<UnsubscribeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<UnsubscribeOptions>().BindConfiguration("OoBDev:Communications:Unsubscribe").ValidateDataAnnotations().ValidateOnStart();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<ISuppressionList, InMemorySuppressionList>();
        services.TryAddSingleton<UnsubscribeLinks>();
        return services;
    }
}
