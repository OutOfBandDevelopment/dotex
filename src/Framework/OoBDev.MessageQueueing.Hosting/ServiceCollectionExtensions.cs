using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.MessageQueueing.Services;

namespace OoBDev.MessageQueueing.Hosting;

/// <summary>
/// Provides extension methods for configuring IoC (Inversion of Control) services
/// to support all Message Queueing within this library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add IOC configurations to support all Message Queueing within this library.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddMessageQueueingHosting(this IServiceCollection services)
    {
        //Note: this is the service host to enable the inbound message handlers
        services.AddOptions();
        services.AddHostedService<MessageReceiverHost>();
        return services;
    }

    /// <summary>
    /// Adds the hosted service that publishes outbox entries. Register <see cref="IOutboxStore"/> (a database store for
    /// durability) and the real <see cref="IMessageQueueSender"/> the dispatcher sends through; the in-memory store is the default.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddOutboxDispatcherHosting(this IServiceCollection services)
    {
        services.AddOptions<OutboxDispatcherHostOptions>();
        services.TryAddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.TryAddSingleton<OutboxDispatcher>();
        services.AddHostedService<OutboxDispatcherHost>();
        return services;
    }
}
