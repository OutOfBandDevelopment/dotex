using OoBDev.System.Text;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;
/// <summary>
/// Provides handling of queue messages by coordinating multiple <see cref="IMessageQueueHandler"/> instances.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageHandlerProvider"/> class.
/// </remarks>
/// <param name="serializer">The JSON serializer.</param>
/// <param name="context">The factory for creating instances of <see cref="IMessageContext"/>.</param>
/// <param name="logger">The logger for logging messages.</param>
public class MessageHandlerProvider(
    IJsonSerializer serializer,
    IMessageContextFactory context,
    ILogger<MessageHandlerProvider> logger
        ) : IMessageHandlerProvider, IMessageHandlerProviderWrapped
{
    private readonly ISerializer _serializer = serializer;
    private readonly IMessageContextFactory _context = context;
    private readonly ILogger _logger = logger;

    private Type? _channelType;
    private IConfigurationSection _config = null!;
    private readonly ConcurrentBag<IMessageQueueHandler> _handlers = [];

    /// <summary>
    /// Gets the configuration section associated with the message handler.
    /// </summary>
    public IConfigurationSection Config => _config ?? throw new ApplicationException($"Missing Configuration");

    /// <summary>
    /// Handles the specified queue message by invoking each registered message handler.
    /// </summary>
    /// <param name="message">The queue message to handle.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task HandleAsync(IQueueMessage message, string messageId)
    {
        if (message != null)
        {
            var context = _context.Create(
                _channelType ?? throw new ApplicationException("No channel type"),
                message,
                Config ?? throw new ApplicationException("No channel configuration")
                );

            var payloadType = message.PayloadType == null ? null : Type.GetType(message.PayloadType);
            var payload = message.Payload;
            if (payloadType != null)
            {
                var convert = _serializer.Serialize(payload, payload.GetType());
                payload = _serializer.Deserialize(convert, payloadType) ?? payload;
            }

            _logger.LogInformation($"Handle: {{{nameof(messageId)}}}", messageId);
            context.SentId = messageId;

            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(payload, context);
            }

            _logger.LogInformation($"Handled: {{{nameof(messageId)}}}", messageId);
        }
        else
        {
            _logger.LogWarning($"Nothing to handle");
        }
    }

    IMessageHandlerProviderWrapped IMessageHandlerProviderWrapped.SetHandlers(IEnumerable<IMessageQueueHandler> handlers)
    {
        foreach (var handler in handlers)
            _handlers.Add(handler);
        return this;
    }

    IMessageHandlerProviderWrapped IMessageHandlerProviderWrapped.SetChannelType(Type channelType)
    {
        _channelType = channelType;
        return this;
    }

    IMessageHandlerProviderWrapped IMessageHandlerProviderWrapped.SetConfig(IConfigurationSection config)
    {
        _config = config ?? throw new ApplicationException($"Missing Configuration");
        return this;
    }

}
