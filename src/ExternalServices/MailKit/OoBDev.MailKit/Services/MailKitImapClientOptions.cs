using MailKit.Security;
using System;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Represents options for configuring a MailKit IMAP client.
/// </summary>
public class MailKitImapClientOptions
{
    /// <summary>
    /// Gets or sets the host address of the IMAP server.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Gets or sets the port number for the IMAP server.
    /// </summary>
    public int Port { get; set; } = 143;

    /// <summary>
    /// Gets or sets the secure socket options for the IMAP connection.
    /// </summary>
    public SecureSocketOptions SecureSocketOption { get; set; }

    /// <summary>
    /// Gets or sets the URI of the IMAP server.
    /// </summary>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Gets or sets the password used for authentication with the IMAP server.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the username used for authentication with the IMAP server.
    /// </summary>
    public string? UserName { get; set; }
}
