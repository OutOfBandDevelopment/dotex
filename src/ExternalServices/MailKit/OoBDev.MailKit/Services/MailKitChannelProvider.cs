using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;

namespace OoBDev.MailKit.Services;

/// <summary>Sends <see cref="IEmailMessage"/> instances over SMTP as the <c>mailkit</c> provider of the <c>email</c> protocol.</summary>
public sealed partial class MailKitChannelProvider(
    IMimeMessageFactory mime,
    ISmtpClientFactory smtp,
    TimeProvider timeProvider,
    ILogger<MailKitChannelProvider> log) : IChannelProvider
{
    /// <inheritdoc />
    public string ProviderName => MailKitGlobals.ProviderName;

    /// <inheritdoc />
    public string[] SupportedProtocols { get; } = [MailKitGlobals.EmailProtocol];

    /// <inheritdoc />
    public Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
        => Task.FromResult(message is IEmailMessage);

    /// <inheritdoc />
    public async Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message is not IEmailMessage email)
        {
            return Failed(message, SendFailureKind.Rejected, "The message is not an email.");
        }

        try
        {
            var model = new EmailMessageModel
            {
                ReferenceId = email.MessageId,
                FromAddress = email.From,
                ToAddresses = email.To,
                CcAddresses = email.Cc,
                BccAddresses = email.Bcc,
                Subject = email.Subject,
                TextContent = email.TextContent,
                HtmlContent = email.HtmlContent,
            };
            if (!string.IsNullOrWhiteSpace(email.ReplyTo))
            {
                model.Headers["Reply-To"] = email.ReplyTo;
            }

            var mimeMessage = await mime.CreateAsync(model, email.Attachments, cancellationToken).ConfigureAwait(false);
            using var client = await smtp.CreateAsync().ConfigureAwait(false);
            var reference = await client.SendAsync(mimeMessage, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            LogSent(email.MessageId, channel.Name);
            return new SendResult(true, email.MessageId, email.CorrelationId, reference, timeProvider.GetUtcNow());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var kind = Classify(ex);
            LogFailed(ex, email.MessageId, channel.Name, kind);
            return Failed(message, kind, ex.Message, (ex as SmtpCommandException)?.StatusCode is { } code ? (int)code : null);
        }
    }

    internal static SendFailureKind Classify(Exception exception) => exception switch
    {
        SmtpCommandException { StatusCode: SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable or SmtpStatusCode.InsufficientStorage or SmtpStatusCode.ServiceNotAvailable or SmtpStatusCode.ErrorInProcessing } => SendFailureKind.Transient,
        SmtpCommandException => SendFailureKind.Permanent,
        AuthenticationException => SendFailureKind.Permanent,
        FormatException or NotSupportedException => SendFailureKind.Permanent,
        ServiceNotConnectedException or ServiceNotAuthenticatedException or IOException or SocketException or TimeoutException => SendFailureKind.Transient,
        _ => SendFailureKind.Transient,
    };

    private SendResult Failed(IMessage message, SendFailureKind kind, string text, int? statusCode = null)
        => new(false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(), new SendFailure(kind, text, statusCode));

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent email {MessageId} on channel {Channel}")]
    private partial void LogSent(string messageId, string channel);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Email {MessageId} on channel {Channel} failed ({Kind})")]
    private partial void LogFailed(Exception exception, string messageId, string channel, SendFailureKind kind);
}
