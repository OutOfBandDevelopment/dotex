using System.ComponentModel.DataAnnotations;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>Options of the durable right audit trail (section <c>OoBDev:RightAudit</c>).</summary>
public sealed class RightAuditOptions
{
    /// <summary>Days an entry is kept before the writer purges it.</summary>
    [Range(1, 36500)]
    public int RetentionDays { get; set; } = 365;

    /// <summary>Entries written per transaction.</summary>
    [Range(1, 10000)]
    public int BatchSize { get; set; } = 100;

    /// <summary>Entries the buffer holds; when it is full the newest entry is dropped and counted.</summary>
    [Range(1, 1000000)]
    public int QueueCapacity { get; set; } = 10000;

    /// <summary>Seconds between flushes of a partly filled batch.</summary>
    [Range(1, 3600)]
    public int FlushSeconds { get; set; } = 5;
}
