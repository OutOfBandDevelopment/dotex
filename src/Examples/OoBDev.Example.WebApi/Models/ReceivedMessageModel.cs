using System;

namespace OoBDev.WebApi.Models;

/// <summary>A message the example handler received from a queue.</summary>
public class ReceivedMessageModel
{
    /// <summary>Gets or sets when the message was received.</summary>
    public required DateTimeOffset ReceivedAt { get; set; }

    /// <summary>Gets or sets the correlation id.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Gets or sets the message as text.</summary>
    public required string Message { get; set; }
}
