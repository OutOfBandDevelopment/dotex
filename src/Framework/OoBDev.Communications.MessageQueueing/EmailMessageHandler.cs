using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;

namespace OoBDev.Communications.MessageQueueing;

/// <summary>Sends queued <see cref="EmailMessageModel"/> payloads through <see cref="ISendService"/> on the configured channel.</summary>
public partial class EmailMessageHandler : IMessageQueueHandler<EmailMessageModel, EmailMessageModel>
{
    private readonly ILogger _logger;
    private readonly ISendService? _send;
    private readonly string _channelName;

    /// <summary>Initializes a new instance of the <see cref="EmailMessageHandler"/> class.</summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The handler settings (channel name).</param>
    /// <param name="send">The send service; when absent, messages are logged and dropped.</param>
    public EmailMessageHandler(
        ILogger<EmailMessageHandler> logger,
        IOptions<EmailMessageHandlerOptions> options,
        ISendService? send = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _channelName = options.Value.ChannelName;
        _send = send;
        if (send == null)
        {
            LogNoProvider();
        }
    }

    /// <summary>Maps the payload to an <see cref="EmailMessage"/> and sends it; a failed send throws so the queue can retry or dead-letter.</summary>
    /// <param name="message">The email message to handle.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">The send did not succeed.</exception>
    public virtual async Task HandleAsync(EmailMessageModel message, IMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);
        if (_send == null)
        {
            LogUnableToSend(message.Subject, message.FromAddress, message.ReferenceId);
            return;
        }

        var id = message.ReferenceId ?? context.CorrelationId;
        LogSending(message.Subject, message.FromAddress, id);
        var email = new EmailMessage
        {
            MessageId = id ?? Guid.NewGuid().ToString("N"),
            CorrelationId = context.CorrelationId,
            From = message.FromAddress,
            To = [.. message.ToAddresses],
            Cc = [.. message.CcAddresses],
            Bcc = [.. message.BccAddresses],
            ReplyTo = message.Headers.TryGetValue("Reply-To", out var replyTo) ? replyTo : null,
            Subject = message.Subject ?? string.Empty,
            TextContent = message.TextContent,
            HtmlContent = message.HtmlContent,
        };

        var result = await _send.SendAsync(_channelName, email).ConfigureAwait(false);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Email {email.MessageId} was not sent on channel '{_channelName}': {result.Failure?.Kind} {result.Failure?.Message}");
        }
    }

    /// <summary>Handles the specified message asynchronously; other payload types are ignored.</summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task HandleAsync(object message, IMessageContext context) =>
        message is EmailMessageModel messageModel ? HandleAsync(messageModel, context) : Task.CompletedTask;

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "No ISendService is registered; queued emails will be dropped")]
    private partial void LogNoProvider();

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "No ISendService is registered. Unable to send {subject} for {from} [{id}]")]
    private partial void LogUnableToSend(string? subject, string? from, string? id);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Sending {subject} for {from} [{id}]")]
    private partial void LogSending(string? subject, string? from, string? id);
}
