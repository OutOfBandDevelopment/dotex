using System;

namespace OoBDev.ComplexEvents;

/// <summary>Declares the default recurrence of an event type as a five-field UTC cron expression; configuration can override it.</summary>
/// <param name="cron">Minute, hour, day of month, month and day of week, for example <c>*/15 * * * *</c>.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ScheduleAtAttribute(string cron) : Attribute
{
    /// <summary>The cron expression.</summary>
    public string Cron { get; } = cron;

    /// <summary>What to do with occurrences missed while the scheduler was down.</summary>
    public CatchUpPolicy CatchUp { get; init; } = CatchUpPolicy.Skip;
}
