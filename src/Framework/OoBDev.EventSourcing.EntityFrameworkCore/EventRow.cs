using System;

namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// The row of the <c>Events</c> table. <see cref="Position"/> is the identity column that gives the global order.
/// </summary>
public sealed class EventRow
{
    /// <summary>Gets or sets the global position (identity).</summary>
    public long Position { get; set; }

    /// <summary>Gets or sets the stream id.</summary>
    public string StreamId { get; set; } = string.Empty;

    /// <summary>Gets or sets the sequence within the stream, starting at 1.</summary>
    public long Sequence { get; set; }

    /// <summary>Gets or sets the event type name.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the time the event was stored.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Gets or sets the JSON payload.</summary>
    public string Payload { get; set; } = "{}";

    /// <summary>Gets or sets the optional JSON metadata.</summary>
    public string? Metadata { get; set; }
}
