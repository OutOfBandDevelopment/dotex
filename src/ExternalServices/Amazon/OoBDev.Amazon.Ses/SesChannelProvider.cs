using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Amazon.Ses;

/// <summary>Sends <see cref="IEmailMessage"/> instances through AWS SES v2 as the <c>aws-ses</c> provider of the <c>email</c> protocol.</summary>
public sealed partial class SesChannelProvider(
    ISesClientFactory clients,
    IOptions<SesOptions> options,
    TimeProvider timeProvider,
    ILogger<SesChannelProvider> log) : IChannelProvider
{
    /// <inheritdoc />
    public string ProviderName => AwsSesGlobals.ProviderName;

    /// <inheritdoc />
    public string[] SupportedProtocols { get; } = [AwsSesGlobals.EmailProtocol];

    /// <inheritdoc />
    public Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
        => Task.FromResult(message is IEmailMessage);

    /// <inheritdoc />
    public async Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(message);
        if (message is not IEmailMessage email)
        {
            return Failed(message, SendFailureKind.Rejected, "The message is not an email.");
        }

        try
        {
            var request = await BuildAsync(email, cancellationToken).ConfigureAwait(false);
            using var client = clients.Create();
            var response = await client.SendEmailAsync(request, cancellationToken).ConfigureAwait(false);

            LogSent(email.MessageId, channel.Name);
            return new SendResult(true, email.MessageId, email.CorrelationId, response.MessageId, timeProvider.GetUtcNow());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var kind = Classify(ex);
            LogFailed(ex, email.MessageId, channel.Name, kind);
            return Failed(message, kind, ex.Message, (ex as AmazonServiceException)?.StatusCode is { } code ? (int)code : null);
        }
    }

    internal async Task<SendEmailRequest> BuildAsync(IEmailMessage email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email.TextContent) && string.IsNullOrWhiteSpace(email.HtmlContent))
        {
            throw new NotSupportedException("You must provide Text or Html Body");
        }

        var from = email.From ?? options.Value.DefaultFromAddress
            ?? throw new NotSupportedException("The message has no sender and SesOptions.DefaultFromAddress is empty.");

        var body = new Body();
        if (!string.IsNullOrWhiteSpace(email.TextContent))
        {
            body.Text = new Content { Data = email.TextContent, Charset = "UTF-8" };
        }

        if (!string.IsNullOrWhiteSpace(email.HtmlContent))
        {
            body.Html = new Content { Data = email.HtmlContent, Charset = "UTF-8" };
        }

        var simple = new Message { Subject = new Content { Data = email.Subject, Charset = "UTF-8" }, Body = body };
        foreach (var attachment in email.Attachments)
        {
            var content = await attachment.GetContentAsync(cancellationToken).ConfigureAwait(false);
            simple.Attachments ??= [];
            simple.Attachments.Add(new Attachment
            {
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                ContentId = attachment.ContentId,
                ContentDisposition = attachment.ContentId is null ? AttachmentContentDisposition.ATTACHMENT : AttachmentContentDisposition.INLINE,
                ContentTransferEncoding = AttachmentContentTransferEncoding.BASE64,
                RawContent = new MemoryStream(content.ToArray()),
            });
        }

        var request = new SendEmailRequest
        {
            FromEmailAddress = from,
            Destination = new Destination
            {
                ToAddresses = [.. email.To],
                CcAddresses = [.. email.Cc],
                BccAddresses = [.. email.Bcc],
            },
            Content = new EmailContent { Simple = simple },
            ConfigurationSetName = options.Value.ConfigurationSetName,
        };
        if (!string.IsNullOrWhiteSpace(email.ReplyTo))
        {
            request.ReplyToAddresses = [email.ReplyTo];
        }

        if (!string.IsNullOrWhiteSpace(options.Value.ConfigurationSetName))
        {
            request.EmailTags = [new MessageTag { Name = "message-id", Value = email.MessageId }];
        }

        return request;
    }

    internal static SendFailureKind Classify(Exception exception) => exception switch
    {
        TooManyRequestsException or LimitExceededException => SendFailureKind.RateLimited,
        MessageRejectedException or MailFromDomainNotVerifiedException or SendingPausedException or AccountSuspendedException => SendFailureKind.Rejected,
        BadRequestException or NotFoundException => SendFailureKind.Permanent,
        NotSupportedException or FormatException => SendFailureKind.Permanent,
        AmazonServiceException { StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized } => SendFailureKind.Permanent,
        _ => SendFailureKind.Transient,
    };

    private SendResult Failed(IMessage message, SendFailureKind kind, string text, int? statusCode = null)
        => new(false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(), new SendFailure(kind, text, statusCode));

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent email {MessageId} on channel {Channel} through SES")]
    private partial void LogSent(string messageId, string channel);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "SES send of {MessageId} on channel {Channel} failed ({Kind})")]
    private partial void LogFailed(Exception exception, string messageId, string channel, SendFailureKind kind);
}
