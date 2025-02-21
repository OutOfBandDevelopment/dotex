using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Models;

/// <summary>
/// Represents a model for SMS messages.
/// </summary>
public class SmsMessageModel
{
    /// <summary>
    /// Gets or sets the phone number or identifier of the sender.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the phone number or identifier of the recipient.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the SMS request.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the body or content of the SMS message.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the headers associated with the SMS message.
    /// </summary>
    public Dictionary<string, object> Headers { get; set; } = [];
}
