Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```WrappedQueueMessage.cs
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents a wrapped queue message.
/// </summary>
public record WrappedQueueMessage : IQueueMessage
{
    /// <summary>
    /// Gets or initializes the content type of the message.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the correlation ID of the message.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Gets or initializes the payload of the message.
    /// </summary>
    public required object Payload { get; init; }

    /// <summary>
    /// Gets or initializes the payload type of the message.
    /// </summary>
    public required string PayloadType { get; init; }

    /// <summary>
    /// Gets or initializes the properties associated with the message.
    /// </summary>
    public required Dictionary<string, object?> Properties { get; init; }
}

```

