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

        services.TryAddSingleton<InProcessMessageProvider>();

        services.AddTransient<IMessageSenderProvider>(sp => sp.GetRequiredService<InProcessMessageProvider>());
        services.TryAddKeyedTransient<IMessageSenderProvider>(
            InProcessMessageProvider.MessageProviderKey,
            (sp, _) => sp.GetRequiredService<InProcessMessageProvider>()
            );

        services.AddTransient<IMessageReceiverProvider>(sp => sp.GetRequiredService<InProcessMessageProvider>());
        services.TryAddKeyedTransient<IMessageReceiverProvider>(
            InProcessMessageProvider.MessageProviderKey,
            (sp, _) => sp.GetRequiredService<InProcessMessageProvider>()
            );

        return services;
    }
}
