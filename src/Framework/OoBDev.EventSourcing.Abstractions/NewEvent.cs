namespace OoBDev.EventSourcing;

/// <summary>
/// An event to append; the store assigns the sequence, timestamp and position.
/// </summary>
/// <param name="Type">The event type name.</param>
/// <param name="Payload">The event body, as JSON text.</param>
/// <param name="Metadata">Optional metadata, as JSON text.</param>
public sealed record NewEvent(string Type, string Payload, string? Metadata = null);
