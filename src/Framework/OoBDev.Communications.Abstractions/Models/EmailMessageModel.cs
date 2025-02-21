using System.Collections.Generic;

namespace OoBDev.Communications.Models;

/// <summary>
/// Represents an email message model.
/// </summary>
public class EmailMessageModel
{
    /// <summary>
    /// Gets or sets the reference ID of the email message.
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the sender's email address.
    /// </summary>
    public string? FromAddress { get; set; }

    /// <summary>
    /// Gets or sets the list of recipient email addresses.
    /// </summary>
    public List<string> ToAddresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of carbon copy (CC) email addresses.
    /// </summary>
    public List<string> CcAddresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of blind carbon copy (BCC) email addresses.
    /// </summary>
    public List<string> BccAddresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the subject of the email message.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the plain text content of the email message.
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Gets or sets the HTML content of the email message.
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Gets or sets the headers of the email message.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of attachment references in the email message.
    /// </summary>
    public List<AttachmentReferenceModel> Attachments { get; set; } = [];
}
