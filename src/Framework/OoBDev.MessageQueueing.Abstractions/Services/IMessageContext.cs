using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;
/// <summary>
/// Represents the context of a message being processed in a message queue.
/// </summary>
public interface IMessageContext
{
    /// <summary>
    /// Gets the origin message identifier.
    /// </summary>
    string? OriginMessageId { get; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    string? RequestId { get; }

    /// <summary>
    /// Gets or sets the sent identifier.
    /// </summary>
    string? SentId { get; set; }

    /// <summary>
    /// Gets the type of the channel.
    /// </summary>
    string? ChannelType { get; }

    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    string? MessageType { get; }

    /// <summary>
    /// Gets the timestamp when the message was sent.
    /// </summary>
    DateTimeOffset? SentAt { get; }

    /// <summary>
    /// Gets the entity that sent the message.
    /// </summary>
    string? SentBy { get; }

    /// <summary>
    /// Gets the entity from which the message was sent.
    /// </summary>
    string? SentFrom { get; }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    object? this[string key] { get; set; }

    /// <summary>
    /// Gets the collection of headers associated with the message.
    /// </summary>
    Dictionary<string, object?> Headers { get; }

    /// <summary>
    /// Gets the configuration section associated with the message context.
    /// </summary>
    IConfigurationSection Config { get; }
}
