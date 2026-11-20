using System;

namespace OoBDev.WebApi.Models;

/// <summary>One email the inbox poll fetched, reduced to what the inbox screen lists.</summary>
public class InboxMessageModel
{
    /// <summary>Gets or sets the message id.</summary>
    public required string MessageId { get; set; }

    /// <summary>Gets or sets the sender address.</summary>
    public string? From { get; set; }

    /// <summary>Gets or sets the recipient addresses.</summary>
    public string[] To { get; set; } = [];

    /// <summary>Gets or sets the subject.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Gets or sets the first characters of the text body.</summary>
    public string Preview { get; set; } = string.Empty;

    /// <summary>Gets or sets when the message was sent.</summary>
    public DateTimeOffset Timestamp { get; set; }
}
