using OoBDev.Communications.Models;
using OoBDev.Communications.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Implementation of <see cref="ICommunicationSender{TMessage}"/> for sending email messages using MailKit.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="MailKitProvider"/> class.
/// </remark>
/// <param name="email">The factory for creating <see cref="MimeMessage"/> instances.</param>
/// <param name="smtp">The factory for creating <see cref="SmtpClient"/> instances.</param>
/// <param name="log">The logger for logging messages.</param>
public partial class MailKitProvider(
    IMimeMessageFactory email,
    ISmtpClientFactory smtp,
    ILogger<MailKitProvider> log
        ) : ICommunicationSender<EmailMessageModel>
{
    /// <summary>
    /// Sends an email asynchronously using MailKit.
    /// </summary>
    /// <param name="message">The email message to be sent.</param>
    /// <returns>A task representing the asynchronous operation, containing a reference or identifier for the sent email.</returns>
    public async Task<string> SendAsync(EmailMessageModel message)
    {
        var model = email.Create(message);
        using var client = await smtp.CreateAsync().ConfigureAwait(false);

        var reference = await client.SendAsync(model).ConfigureAwait(false);
        await client.DisconnectAsync(true).ConfigureAwait(false);

        LogSent(message.Subject, message.ToAddresses, reference);

        return reference;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent email \"{Subject}\" to \"{ToAddresses}\" [{reference}]")]
    private partial void LogSent(string? subject, IList<string> toAddresses, string reference);
}
