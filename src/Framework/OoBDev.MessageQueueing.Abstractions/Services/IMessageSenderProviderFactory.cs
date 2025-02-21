using System;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents a factory for creating instances of <see cref="IMessageSenderProvider"/>.
/// </summary>
public interface IMessageSenderProviderFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IMessageSenderProvider"/> for the specified channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>An instance of <see cref="IMessageSenderProvider"/>.</returns>
    IMessageSenderProvider Sender(Type channelType, Type messageType);
}
