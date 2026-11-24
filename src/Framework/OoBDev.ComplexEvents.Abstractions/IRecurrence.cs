using System;

namespace OoBDev.ComplexEvents;

/// <summary>Calculates when a recurring schedule is due next (cron, RRule or any other rule).</summary>
public interface IRecurrence
{
    /// <summary>The first occurrence strictly after <paramref name="after"/>, or null when the rule has ended.</summary>
    /// <param name="after">The moment to search after.</param>
    DateTimeOffset? Next(DateTimeOffset after);
}
