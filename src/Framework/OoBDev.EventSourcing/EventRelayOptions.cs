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
    /// Events newer than this are left for the next pass, because an identity position can commit out of order.
    /// </summary>
    public TimeSpan SafetyWindow { get; set; } = TimeSpan.FromSeconds(5);
}
