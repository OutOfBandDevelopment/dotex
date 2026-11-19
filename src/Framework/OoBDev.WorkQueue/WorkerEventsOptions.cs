using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.WorkQueue;

/// <summary>
/// Retention of the worker event history.
/// </summary>
public sealed class WorkerEventsOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "WorkQueue:Events";

    /// <summary>The most events kept; older ones are trimmed.</summary>
    [Range(1, 10_000_000)]
    public int MaxRecords { get; set; } = 10_000;

    /// <summary>The longest an event is kept.</summary>
    public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(7);
}
