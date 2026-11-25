using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>An email.</summary>
public interface IEmailMessage : IMessage
{
    /// <summary>Gets the recipients.</summary>
    string[] To { get; }

    /// <summary>Gets the carbon-copy recipients.</summary>
    string[] Cc { get; }

    /// <summary>Gets the blind-carbon-copy recipients.</summary>
    string[] Bcc { get; }

    /// <summary>Gets the sender, or <c>null</c> to use the provider default.</summary>
    string? From { get; }

    /// <summary>Gets the reply-to address.</summary>
    string? ReplyTo { get; }

    /// <summary>Gets the subject.</summary>
    string Subject { get; }

    /// <summary>Gets the plain text body.</summary>
    string? TextContent { get; }

    /// <summary>Gets the HTML body.</summary>
    string? HtmlContent { get; }

    /// <summary>Gets the attachments.</summary>
    IReadOnlyList<IEmailAttachment> Attachments { get; }
}
