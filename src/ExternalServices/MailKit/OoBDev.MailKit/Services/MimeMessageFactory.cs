using OoBDev.Communications.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Linq;

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
    public MimeMessage Create(EmailMessageModel message)
    {
        var email = new MimeMessage
        {
            From =
            {
                InternetAddress.Parse(message.FromAddress ?? config.Value.DefaultFromEmailAddress),
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

        email.Subject = message.Subject;

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
        email.Body = bodyBuilder.ToMessageBody();

        //TODO: add attachment support

        return email;
    }

    /// <summary>
    /// Creates a <see cref="ReceivedEmailMessageModel"/> from the specified <see cref="MimeMessage"/>.
    /// and inbound metadata such as host and mailbox path.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="server"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ReceivedEmailMessageModel ToReceived(MimeMessage message, string server, string path)
    {
        var received = new ReceivedEmailMessageModel
        {
            FromAddress = string.Join(';', message.From.Cast<InternetAddress>()),
            Subject = message.Subject,
            TextContent = message.TextBody,
            HtmlContent = message.HtmlBody,
            ReferenceId = message.MessageId,

            Server = server,
            Path = path,
        };
        received.ToAddresses.AddRange(message.To.Cast<InternetAddress>().Select(a => a.ToString()));
        received.CcAddresses.AddRange(message.Cc.Cast<InternetAddress>().Select(a => a.ToString()));
        received.BccAddresses.AddRange(message.Bcc.Cast<InternetAddress>().Select(a => a.ToString()));

        foreach (var header in message.Headers)
        {
            received.Headers.Add(header.Field, header.Value);
        }

        return received;
    }

}
