using MailKit.Security;
using System;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Represents options for configuring a MailKit SMTP client.
/// </summary>
public class MailKitSmtpClientOptions
{
    /// <summary>
    /// Gets or sets the host address of the SMTP server.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Gets or sets the port number for the SMTP server.
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// Gets or sets the secure socket options for the SMTP connection.
    /// </summary>
    public SecureSocketOptions SecureSocketOption { get; set; }

    /// <summary>
    /// Gets or sets the URI of the SMTP server.
    /// </summary>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Gets or sets the password used for authentication with the SMTP server.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the username used for authentication with the SMTP server.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the default email address to use as the sender in outgoing emails.
    /// </summary>
    public string? DefaultFromEmailAddress { get; set; }
}
