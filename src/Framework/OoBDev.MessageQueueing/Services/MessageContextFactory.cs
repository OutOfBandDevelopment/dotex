using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Security.Claims;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Factory for creating instances of <see cref="IMessageContext"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageContextFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider for creating instances.</param>
/// <param name="user">The claims principal representing the user associated with the message context.</param>
public class MessageContextFactory(
    IServiceProvider serviceProvider,
    ClaimsPrincipal? user
        ) : IMessageContextFactory
{

    /// <summary>
    /// Creates a new instance of <see cref="IMessageContext"/> with the specified parameters.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="originMessageId">The origin message ID.</param>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <param name="caller">The calling method.</param>
    /// <param name="callerLine">The line number in the source file where the call originated.</param>
    /// <param name="callerFile">The full path of the source file where the call originated.</param>
    /// <returns>A new instance of <see cref="IMessageContext"/>.</returns>
    public virtual IMessageContext Create(
        Type channelType,
        Type messageType,
        string? originMessageId,
        string correlationId,
        string requestId,
        IConfigurationSection configuration,
        MethodBase? caller,
        int callerLine,
        string? callerFile
        )
    {
        var context = ActivatorUtilities.CreateInstance<MessageContext>(serviceProvider);

        var userName = (user ?? ClaimsPrincipal.Current)?.Identity?.Name ?? Environment.UserName;

        context.OriginMessageId = originMessageId;
        context.CorrelationId = correlationId;
        context.RequestId = requestId;

        context.SentBy = userName;
        context.SentFrom = Environment.MachineName;
        context.SentAt = DateTimeOffset.UtcNow;

        context.ChannelType = channelType.AssemblyQualifiedName;
        context.MessageType = messageType.AssemblyQualifiedName;

        context["X-CallerName"] = caller?.DeclaringType?.AssemblyQualifiedName ?? "UNKNOWN CALLER";
        context["X-CallerMemberName"] = caller?.ToString() ?? "UNKNOWN CALLER";
        context["X-CallerLineNumber"] = callerLine;
        context["X-CallerFilePath"] = callerFile ?? "UNKNOWN CALLER PATH";

        context.Config = configuration;

        return context;
    }

    /// <summary>
    /// Creates a new instance of <see cref="IMessageContext"/> with the specified parameters from a queue message.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="message">The queue message.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>A new instance of <see cref="IMessageContext"/>.</returns>
    public virtual IMessageContext Create(Type channelType, IQueueMessage message, IConfigurationSection configuration)
    {
        var context = ActivatorUtilities.CreateInstance<MessageContext>(serviceProvider);

        context.ChannelType = channelType.AssemblyQualifiedName;
        context.Config = configuration;

        foreach (var item in message.Properties)
        {
            context[item.Key] = item.Value;
        }

        return context;
    }
}
