using System;

namespace OoBDev.ComplexEvents.Hosting;

/// <summary>Options for <see cref="ScheduleHost"/>.</summary>
public sealed class ScheduleHostOptions
{
    /// <summary>How often due items are looked for.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Most items leased per poll.</summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>How long a lease holds an item before another instance may take it.</summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(5);
}
