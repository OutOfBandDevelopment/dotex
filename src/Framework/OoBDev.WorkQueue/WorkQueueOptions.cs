using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.WorkQueue;

/// <summary>
/// Options of the work queue and its stage workers.
/// </summary>
public sealed class WorkQueueOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "WorkQueue";

    /// <summary>The service level used by stages without an override.</summary>
    public StageSla Default { get; set; } = new();

    /// <summary>Per-stage overrides, for example a longer lease for a slow stage.</summary>
    public Dictionary<string, StageSla> Stages { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>First retry delay; it doubles with every attempt up to <see cref="BackoffMax"/>.</summary>
    public TimeSpan BackoffBase { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Longest retry delay.</summary>
    public TimeSpan BackoffMax { get; set; } = TimeSpan.FromHours(1);

    /// <summary>How many items a stage worker claims at once.</summary>
    [Range(1, 1000)]
    public int BatchSize { get; set; } = 1;

    /// <summary>How long an idle stage worker waits before it asks for work again.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>How often a stage worker extends the lease while the handler runs; null means a third of the lease.</summary>
    public TimeSpan? HeartbeatInterval { get; set; }

    /// <summary>The service level of a stage.</summary>
    /// <param name="stage">The stage name.</param>
    /// <returns>The override when there is one, otherwise <see cref="Default"/>.</returns>
    public StageSla For(string stage) => Stages.TryGetValue(stage, out var sla) ? sla : Default;

    /// <summary>The retry delay after a failed attempt: <see cref="BackoffBase"/> doubled per earlier attempt, capped at <see cref="BackoffMax"/>.</summary>
    /// <param name="attempt">The attempt that failed (1 for the first).</param>
    /// <returns>The delay before the item can be claimed again.</returns>
    public TimeSpan Backoff(int attempt) =>
        TimeSpan.FromTicks(Math.Min(BackoffMax.Ticks, BackoffBase.Ticks * (1L << Math.Clamp(attempt - 1, 0, 20))));
}
