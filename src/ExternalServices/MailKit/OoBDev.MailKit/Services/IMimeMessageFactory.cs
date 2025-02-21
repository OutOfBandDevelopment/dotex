using OoBDev.Communications.Models;
using MimeKit;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Represents a factory for creating <see cref="MimeMessage"/> instances from <see cref="EmailMessageModel"/>.
/// </summary>
public interface IMimeMessageFactory
{
    /// <summary>
    /// Creates a <see cref="MimeMessage"/> from the specified <see cref="EmailMessageModel"/>.
    /// </summary>
    /// <param name="message">The email message model.</param>
    /// <returns>A <see cref="MimeMessage"/> instance.</returns>
    MimeMessage Create(EmailMessageModel message);

    /// <summary>
    /// Creates a <see cref="ReceivedEmailMessageModel"/> from the specified <see cref="MimeMessage"/>.
    /// and inbound metadata such as host and mailbox path.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="server"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    ReceivedEmailMessageModel ToReceived(MimeMessage message, string server, string path);
}
