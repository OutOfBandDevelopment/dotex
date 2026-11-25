using System;

namespace OoBDev.EventSourcing;

/// <summary>
/// A stored event. Correlation and caller information travel in the message context, not here.
/// </summary>
/// <param name="StreamId">The stream (usually one aggregate) the event belongs to.</param>
/// <param name="Sequence">The one-based position of the event inside its stream.</param>
/// <param name="Type">The event type name.</param>
/// <param name="Timestamp">When the event was appended.</param>
/// <param name="Payload">The event body, as JSON text.</param>
/// <param name="Metadata">Optional metadata, as JSON text.</param>
/// <param name="Position">The global position in the store; zero until the store assigns it.</param>
public sealed record EventRecord(
    string StreamId,
    long Sequence,
    string Type,
    DateTimeOffset Timestamp,
    string Payload,
    string? Metadata = null,
    long Position = 0);
