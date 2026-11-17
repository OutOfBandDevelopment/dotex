using System;

namespace OoBDev.WorkQueue;

/// <summary>
/// A claim on one work item. <see cref="Attempt"/> and <see cref="Worker"/> fence late calls from a worker whose lease was lost.
/// </summary>
/// <param name="Stage">The stage the item belongs to.</param>
/// <param name="Key">The item key, unique within the stage (for example a content hash or a path).</param>
/// <param name="Attempt">The attempt number this claim started (1 for the first claim).</param>
/// <param name="Worker">The worker that holds the claim.</param>
/// <param name="LeaseUntil">When the lease ends unless a heartbeat extends it.</param>
public sealed record WorkItem(string Stage, string Key, int Attempt, string Worker, DateTimeOffset LeaseUntil);
