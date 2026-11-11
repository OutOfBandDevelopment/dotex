using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.EventSourcing;

/// <summary>
/// Options of the <see cref="EventRelay"/>.
/// </summary>
public sealed class EventRelayOptions
{
    /// <summary>The checkpoint name; one relay per name.</summary>
    [Required]
    public string Name { get; set; } = "event-relay";

    /// <summary>The most events read per pass.</summary>
    [Range(1, 10_000)]
    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// The pause between passes of the hosted relay when no events are waiting.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "01:00:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Events newer than this are left for the next pass, because an identity position can commit out of order.
    /// </summary>
    public TimeSpan SafetyWindow { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the CloudEvents <c>source</c>. When set, the relay publishes each event as a CloudEvents 1.0 JSON string instead of an <see cref="EventRecord"/>.
    /// </summary>
    public string? CloudEventsSource { get; set; }
}
