using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents the context associated with a message, including metadata and headers.
/// </summary>
public class MessageContext : IMessageContext
{
    /// <summary>
    /// Gets or sets the origin message ID.
    /// </summary>
    public string? OriginMessageId
    {
        get => this[$"X-{nameof(OriginMessageId)}"] as string;
        internal set => this[$"X-{nameof(OriginMessageId)}"] = value;
    }

    /// <summary>
    /// Gets or sets the correlation ID.
    /// </summary>
    public string? CorrelationId
    {
        get => this[$"X-{nameof(CorrelationId)}"] as string;
        set => this[$"X-{nameof(CorrelationId)}"] = value;
    }

    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    public string? RequestId
    {
        get => this[$"X-{nameof(RequestId)}"] as string;
        internal set => this[$"X-{nameof(RequestId)}"] = value;
    }

    /// <summary>
    /// Gets or sets the sent ID.
    /// </summary>
    public string? SentId
    {
        get => this[$"X-{nameof(SentId)}"] as string;
        set => this[$"X-{nameof(SentId)}"] = value;
    }

    /// <summary>
    /// Gets or sets the channel type.
    /// </summary>
    public string? ChannelType
    {
        get => this[$"X-{nameof(ChannelType)}"] as string;
        internal set => this[$"X-{nameof(ChannelType)}"] = value;
    }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string? MessageType
    {
        get => this[$"X-{nameof(MessageType)}"] as string;
        internal set => this[$"X-{nameof(MessageType)}"] = value;
    }

    /// <summary>
    /// Gets or sets the timestamp when the message was sent.
    /// </summary>
    public DateTimeOffset? SentAt
    {
        get => this[$"X-{nameof(SentAt)}"] as DateTimeOffset?;
        internal set => this[$"X-{nameof(SentAt)}"] = value;
    }

    /// <summary>
    /// Gets or sets the entity that sent the message.
    /// </summary>
    public string? SentBy
    {
        get => this[$"X-{nameof(SentBy)}"] as string;
        internal set => this[$"X-{nameof(SentBy)}"] = value;
    }

    /// <summary>
    /// Gets or sets the origin from where the message was sent.
    /// </summary>
    public string? SentFrom
    {
        get => this[$"X-{nameof(SentFrom)}"] as string;
        internal set => this[$"X-{nameof(SentFrom)}"] = value;
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key in the headers.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public object? this[string key]
    {
        get => Headers.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (!Headers.TryAdd(key, value))
                Headers[key] = value;
        }
    }

    /// <summary>
    /// Gets the headers associated with the message context.
    /// </summary>
    public Dictionary<string, object?> Headers { get; } = [];

    /// <summary>
    /// Gets or sets the configuration section associated with the message context.
    /// </summary>
    public IConfigurationSection Config { get; internal set; } = null!;
}
