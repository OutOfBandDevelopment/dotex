using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing;

/// <summary>
/// Represents a message sender for a specific communication channel (<typeparamref name="TChannel"/>).
/// </summary>
/// <typeparam name="TChannel">The type representing the communication channel.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageSender{TChannel}"/> class.
/// </remarks>
/// <param name="context">The message context factory.</param>
/// <param name="provider">The message sender provider factory.</param>
/// <param name="resolver">The message property resolver.</param>
/// <param name="logger">The logger.</param>
public class MessageSender<TChannel>(
    IMessageContextFactory context,
    IMessageSenderProviderFactory provider,
    IMessagePropertyResolver resolver,
    ILogger<TChannel> logger
    ) : IMessageQueueSender<TChannel>
{
    private readonly IMessageContextFactory _context = context;
    private readonly IMessageSenderProviderFactory _provider = provider;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends a message asynchronously to the specified communication channel.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="correlationId">The correlation ID associated with the message (optional).</param>
    /// <param name="callerMember">The calling member, supplied by the compiler.</param>
    /// <param name="callerFile">The calling source file, supplied by the compiler.</param>
    /// <param name="callerLine">The calling line number, supplied by the compiler.</param>
    /// <returns>The ID of the sent message.</returns>
    public virtual async Task<string> SendAsync(
        object message,
        string? correlationId = default,
        [CallerMemberName] string? callerMember = default,
        [CallerFilePath] string? callerFile = default,
        [CallerLineNumber] int callerLine = default
    )
    {
        var targetType = typeof(TChannel);
        var messageType = message.GetType();

        var originMessageId = correlationId;
        correlationId = resolver.MessageId(targetType, messageType, correlationId);
        var requestId = resolver.GenerateId(targetType, messageType);
        var config = resolver.Configuration(targetType, messageType);
        var context = _context.Create(
            targetType,
            messageType,
            originMessageId,
            correlationId,
            requestId,
            config,
            callerMember,
            callerLine,
            callerFile
        );
        var provider = _provider.Sender(targetType, messageType);

        _logger.LogInformation("Sending: \"{message}\" [{orgMessageId} -> {messageId}] to \"{targetType}\" from \"{callerFile}::{callerMember}\"",
            message,
            originMessageId,
            correlationId,
            targetType,
            callerFile,
            callerMember
        );

        try
        {
            var sentId = await provider.SendAsync(message, context);

            _logger.LogInformation("Sent: [{orgMessageId} -> {messageId}] => ({sentId})",
                originMessageId,
                correlationId,
                sentId
            );

            context.SentId = sentId;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: \"{message}\" [{orgMessageId} -> {messageId}]",
                ex.Message,
                originMessageId,
                correlationId
            );

            _logger.LogDebug("Exception: {trace}\r\n [{orgMessageId} -> {messageId}]",
                ex.ToString(),
                originMessageId,
                correlationId
            );

            throw;
        }
        return context.SentId ?? context.CorrelationId ?? originMessageId ?? requestId;
    }
}
