using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Implementation of <see cref="IMimeMessageFactory"/> for creating MimeMessage instances for email messages.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="MimeMessageFactory"/> class.
/// </remark>
/// <param name="config">The configuration options for the MailKit SMTP client.</param>
public class MimeMessageFactory(
    IOptions<MailKitSmtpClientOptions> config) : IMimeMessageFactory
{
    /// <summary>
    /// Creates a MimeMessage instance from the specified <see cref="EmailMessageModel"/>.
    /// </summary>
    /// <param name="message">The email message model containing information for creating the MimeMessage.</param>
    /// <returns>A MimeMessage instance representing the email message.</returns>
    public MimeMessage Create(EmailMessageModel message) => Build(message, []);

    /// <inheritdoc />
    public async Task<MimeMessage> CreateAsync(EmailMessageModel message, IReadOnlyList<IEmailAttachment> attachments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachments);
        var resolved = new List<(IEmailAttachment Attachment, ReadOnlyMemory<byte> Content)>(attachments.Count);
        foreach (var attachment in attachments)
        {
            resolved.Add((attachment, await attachment.GetContentAsync(cancellationToken).ConfigureAwait(false)));
        }

        return Build(message, resolved);
    }

    private MimeMessage Build(EmailMessageModel message, IReadOnlyList<(IEmailAttachment Attachment, ReadOnlyMemory<byte> Content)> attachments)
    {
        var email = new MimeMessage
        {
            From =
            {
                InternetAddress.Parse((message.FromAddress ?? config.Value.DefaultFromEmailAddress)!),
            }
        };

        if (!string.IsNullOrWhiteSpace(message.ReferenceId))
        {
            email.Headers.Add($"X-{nameof(message.ReferenceId)}", message.ReferenceId);

        }
        foreach (var header in message.Headers.Where(kvp => kvp.Value != null))
        {
            email.Headers.Add(header.Key, header.Value);
        }

        email.To.AddRange(message.ToAddresses.Select(m => InternetAddress.Parse(m)));
        email.Cc.AddRange(message.CcAddresses.Select(m => InternetAddress.Parse(m)));
        email.Bcc.AddRange(message.BccAddresses.Select(m => InternetAddress.Parse(m)));

        email.Subject = message.Subject ?? string.Empty;

        var bodyBuilder = new BodyBuilder();
        if (string.IsNullOrWhiteSpace(message.TextContent) && string.IsNullOrWhiteSpace(message.HtmlContent))
        {
            throw new NotSupportedException("You must provide Text or Html Body");
        }
        if (!string.IsNullOrWhiteSpace(message.TextContent))
        {
            bodyBuilder.TextBody = message.TextContent;
        }
        if (!string.IsNullOrWhiteSpace(message.HtmlContent))
        {
            bodyBuilder.HtmlBody = message.HtmlContent;
        }
        foreach (var (attachment, content) in attachments)
        {
            var contentType = ContentType.Parse(attachment.ContentType);
            var part = (MimePart)bodyBuilder.Attachments.Add(attachment.FileName, content.ToArray(), contentType);
            part.ContentId = attachment.ContentId;
            if (attachment.ContentId is not null)
            {
                part.ContentDisposition = new ContentDisposition(ContentDisposition.Inline) { FileName = attachment.FileName };
            }
        }

        email.Body = bodyBuilder.ToMessageBody();

        return email;
    }

    /// <summary>
    /// Creates an <see cref="EmailMessage"/> from the specified <see cref="MimeMessage"/>; the host and mailbox path go into <c>Metadata</c> (<c>server</c>, <c>path</c>).
    /// </summary>
    /// <param name="message">The received MIME message.</param>
    /// <param name="server">The server the message came from.</param>
    /// <param name="path">The mailbox path.</param>
    /// <returns>The email message.</returns>
    public EmailMessage ToReceived(MimeMessage message, string server, string path)
    {
        ArgumentNullException.ThrowIfNull(message);
        var received = new EmailMessage
        {
            MessageId = message.MessageId ?? Guid.NewGuid().ToString("N"),
            Timestamp = message.Date,
            From = message.From.Count == 0 ? null : string.Join(';', message.From.Cast<InternetAddress>()),
            To = [.. message.To.Cast<InternetAddress>().Select(a => a.ToString())],
            Cc = [.. message.Cc.Cast<InternetAddress>().Select(a => a.ToString())],
            Bcc = [.. message.Bcc.Cast<InternetAddress>().Select(a => a.ToString())],
            ReplyTo = message.ReplyTo.Count == 0 ? null : string.Join(';', message.ReplyTo.Cast<InternetAddress>()),
            Subject = message.Subject ?? string.Empty,
            TextContent = message.TextBody,
            HtmlContent = message.HtmlBody,
        };
        received.Metadata["server"] = server;
        received.Metadata["path"] = path;
        return received;
    }

}
