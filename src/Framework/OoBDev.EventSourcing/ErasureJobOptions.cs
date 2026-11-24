using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.EventSourcing;

/// <summary>
/// Options of the hosted erasure compaction job.
/// </summary>
public sealed class ErasureJobOptions
{
    /// <summary>
    /// Gets or sets how long the job waits between passes.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "1.00:00:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(1);
}
