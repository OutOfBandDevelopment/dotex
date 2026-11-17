using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Twilio;

/// <summary>Settings of the Twilio provider.</summary>
public sealed class TwilioOptions
{
    /// <summary>Gets or sets the account SID.</summary>
    [Required]
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>Gets or sets the auth token; keep it in configuration or user secrets, never in a file in the repository.</summary>
    [Required]
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender number used when a message has none.</summary>
    public string? DefaultFrom { get; set; }

    /// <summary>Gets or sets the API base address.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.twilio.com/");
}
