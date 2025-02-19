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
public class MessageReceiverProviderFactory(
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
                                let handlerInterfaces = from @interface in handlerType.GetInterfaces()
                                                        where typeof(IMessageQueueHandler).IsAssignableFrom(@interface)
                                                        orderby @interface.GenericTypeArguments.Length descending
                                                        select @interface
                                let handlerInterface = handlerInterfaces.FirstOrDefault()
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
            var handlers = item.Select(i => i.handler).ToList();
            if (item.Key.providerKey == null)
            {
                _logger.LogWarning($"No provider configured for handlers {{{nameof(handlers)}}}", handlers);
                continue;
            }

            var receiver = Receiver(item.Key.providerKey);

            var config = item.First().config.configurationSection;

            var disableReceiverValue = config?["DisableReceiver"];
            var disableReceiver =
                string.Equals("TRUE", disableReceiverValue, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals("1", disableReceiverValue, StringComparison.InvariantCultureIgnoreCase);

            if (disableReceiver)
            {
                _logger.LogWarning($"Provider disabled for handlers {{{nameof(handlers)}}}", handlers);
                continue;
            }

            var handler = serviceProvider.GetRequiredService<IMessageHandlerProvider>();

            if (handler is IMessageHandlerProviderWrapped wrapped)
            {
                wrapped
                .SetHandlers(handlers)
                .SetChannelType(item.Key.channelType)
                .SetConfig(config ?? throw new ApplicationException($"Missing Configuration"));
            }

            receiver.SetHandlerProvider(handler);

            yield return receiver;
        }
    }

    private IMessageReceiverProvider Receiver(string providerKey)
    {
        var provider = serviceProvider.GetKeyedService<IMessageReceiverProvider>(providerKey);

        if (provider == null)
        {
            var providerType = Type.GetType(providerKey, true) ??
                throw new ApplicationException($"Unable to resolve type for {providerKey}");

            provider = (IMessageReceiverProvider)ActivatorUtilities.CreateInstance(serviceProvider, providerType);
        }

        return provider;
    }
}
