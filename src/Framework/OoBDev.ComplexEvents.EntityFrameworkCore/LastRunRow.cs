namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>The last run of one recurring schedule. Times are UTC ticks, like <see cref="ScheduleRow"/>.</summary>
public sealed class LastRunRow
{
    /// <summary>The recurring schedule's name.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Due time of the last occurrence that ran, as UTC ticks.</summary>
    public long LastRunTicks { get; set; }
}
