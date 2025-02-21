using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.MessageQueueing;

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
    public static IServiceCollection TryAddMessageQueueingServices(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IMessageQueueSender<>), typeof(MessageSender<>));
        services.TryAddTransient<IMessageQueueSender, MessageSender<object>>();

        services.TryAddTransient<IMessageContext, MessageContext>();

        services.TryAddTransient<IMessageContextFactory, MessageContextFactory>();
        services.TryAddTransient<IMessageSenderProviderFactory, MessageSenderProviderFactory>();
        services.TryAddTransient<IMessagePropertyResolver, MessagePropertyResolver>();
        services.TryAddTransient<IMessageHandlerProvider, MessageHandlerProvider>();
        services.TryAddTransient<IMessageReceiverProviderFactory, MessageReceiverProviderFactory>();

        services.AddTransient<IMessageSenderProvider, InProcessMessageProvider>();
        services.TryAddKeyedTransient<IMessageSenderProvider, InProcessMessageProvider>(InProcessMessageProvider.MessageProviderKey);

        return services;
    }
}
