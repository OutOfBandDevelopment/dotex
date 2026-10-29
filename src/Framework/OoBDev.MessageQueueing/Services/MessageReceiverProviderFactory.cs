using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Factory for creating instances of <see cref="IMessageReceiverProvider"/> based on configured message handlers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageReceiverProviderFactory"/> class.
/// </remarks>
/// <param name="handlers">The collection of message queue handlers.</param>
/// <param name="resolver">The message property resolver.</param>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="logger">The logger.</param>
public partial class MessageReceiverProviderFactory(
    IEnumerable<IMessageQueueHandler> handlers,
    IMessagePropertyResolver resolver,
    IServiceProvider serviceProvider,
    ILogger<MessageReceiverProviderFactory> logger
    ) : IMessageReceiverProviderFactory
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Creates instances of <see cref="IMessageReceiverProvider"/> based on configured message handlers.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="IMessageReceiverProvider"/>.</returns>
    public virtual IEnumerable<IMessageReceiverProvider> Create()
    {
        var handlersByChannel = from handler in handlers

                                let handlerType = handler.GetType()
                                let handlerInterface = MostSpecificHandlerInterface(handlerType)
                                where handlerInterface != null

                                let channelType = handlerInterface.GenericTypeArguments.FirstOrDefault() ?? typeof(object)
                                let messageType = handlerInterface.GenericTypeArguments.ElementAtOrDefault(1) ?? typeof(object)

                                let provider = resolver.ProviderSafe(channelType, messageType)
                                let config = resolver.ConfigurationSafe(channelType, messageType)

                                group new
                                {
                                    handler,
                                    handlerInterface,
                                    provider,
                                    config,
                                } by new
                                {
                                    provider.providerKey,
                                    provider.configPath,
                                    channelType,
                                };

        foreach (var item in handlersByChannel)
        {
            var channelHandlers = item.Select(i => i.handler).ToList();
            if (item.Key.providerKey == null)
            {
                LogNoProvider(channelHandlers);
                continue;
            }

            var receiver = Receiver(item.Key.providerKey);

            var config = item.First().config.configurationSection;

            if (IsDisabled(config?["DisableReceiver"]))
            {
                LogProviderDisabled(channelHandlers);
                continue;
            }

            var handler = serviceProvider.GetRequiredService<IMessageHandlerProvider>();

            if (handler is IMessageHandlerProviderWrapped wrapped)
            {
                wrapped
                .SetHandlers(channelHandlers)
                .SetChannelType(item.Key.channelType)
                .SetConfig(config ?? throw new InvalidOperationException($"Missing Configuration"));
            }

            receiver.SetHandlerProvider(handler);

            yield return receiver;
        }
    }

    private static Type? MostSpecificHandlerInterface(Type handlerType) =>
        (from @interface in handlerType.GetInterfaces()
         where typeof(IMessageQueueHandler).IsAssignableFrom(@interface)
         orderby @interface.GenericTypeArguments.Length descending
         select @interface).FirstOrDefault();

    private static bool IsDisabled(string? disableReceiverValue) =>
        string.Equals("TRUE", disableReceiverValue, StringComparison.OrdinalIgnoreCase) ||
        string.Equals("1", disableReceiverValue, StringComparison.OrdinalIgnoreCase);

    private IMessageReceiverProvider Receiver(string providerKey)
    {
        var provider = serviceProvider.GetKeyedService<IMessageReceiverProvider>(providerKey);

        if (provider == null)
        {
            var providerType = Type.GetType(providerKey, true) ??
                throw new InvalidOperationException($"Unable to resolve type for {providerKey}");

            provider = (IMessageReceiverProvider)ActivatorUtilities.CreateInstance(serviceProvider, providerType);
        }

        return provider;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "No provider configured for handlers {handlers}")]
    private partial void LogNoProvider(IEnumerable<IMessageQueueHandler> handlers);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Provider disabled for handlers {handlers}")]
    private partial void LogProviderDisabled(IEnumerable<IMessageQueueHandler> handlers);
}
