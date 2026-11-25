using System;

namespace OoBDev.WorkQueue;

/// <summary>
/// Service level for one stage: how long a step may be silent, how long it may run and how often it may be tried.
/// </summary>
public sealed class StageSla
{
    /// <summary>A claim is valid this long without a heartbeat; after that anyone can claim the item and the attempt counts.</summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Hard ceiling for one attempt even while heartbeats arrive, so a runaway cannot hold the stage forever.</summary>
    public TimeSpan MaxRuntime { get; set; } = TimeSpan.FromHours(2);

    /// <summary>Claims allowed before the item is taken out of the flow (parked if it hung, dead if it kept failing).</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>When false the host does not start a worker for this stage (the stage is enabled by name in configuration).</summary>
    public bool Enabled { get; set; } = true;
}
