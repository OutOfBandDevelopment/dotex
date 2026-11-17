using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.SendGrid;

/// <summary>Settings of the SendGrid provider.</summary>
public sealed class SendGridOptions
{
    /// <summary>Gets or sets the API key; keep it in configuration or user secrets, never in a file in the repository.</summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender used when a message has none.</summary>
    public string? DefaultFrom { get; set; }

    /// <summary>Gets or sets the API base address.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.sendgrid.com/");
}
