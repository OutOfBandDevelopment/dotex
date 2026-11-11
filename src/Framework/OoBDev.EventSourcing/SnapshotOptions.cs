using System.ComponentModel.DataAnnotations;

namespace OoBDev.EventSourcing;

/// <summary>
/// Options for aggregate snapshots.
/// </summary>
public sealed class SnapshotOptions
{
    /// <summary>The snapshot is saved each time the stream version passes a multiple of this; 0 turns snapshots off.</summary>
    [Range(0, 1_000_000)]
    public int Interval { get; set; } = 100;
}
