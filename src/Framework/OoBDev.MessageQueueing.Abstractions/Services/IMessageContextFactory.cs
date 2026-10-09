using Microsoft.Extensions.Configuration;
using System;

namespace OoBDev.MessageQueueing.Services;
/// <summary>
/// Factory for creating instances of <see cref="IMessageContext"/>.
/// </summary>
public interface IMessageContextFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IMessageContext"/> with the specified parameters.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="originMessageId">The origin message identifier.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="configuration">The configuration section associated with the message context.</param>
    /// <param name="callerMember">The name of the calling member (from <c>CallerMemberName</c>).</param>
    /// <param name="callerLine">The line number at which the method is called.</param>
    /// <param name="callerFile">The path to the source file that contains the calling method.</param>
    /// <returns>A new instance of <see cref="IMessageContext"/>.</returns>
    IMessageContext Create(
        Type channelType,
        Type messageType,
        string? originMessageId,
        string correlationId,
        string requestId,
        IConfigurationSection configuration,
        string? callerMember,
        int callerLine,
        string? callerFile
        );

    /// <summary>
    /// Creates a new instance of <see cref="IMessageContext"/> for the given channel and message.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="message">The queue message.</param>
    /// <param name="configuration">The configuration section associated with the message context.</param>
    /// <returns>A new instance of <see cref="IMessageContext"/>.</returns>
    IMessageContext Create(Type channelType, IQueueMessage message, IConfigurationSection configuration);
}
