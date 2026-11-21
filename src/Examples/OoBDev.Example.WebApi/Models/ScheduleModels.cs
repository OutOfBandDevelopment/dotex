using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>Asks for a message to be scheduled.</summary>
public sealed class ScheduleRequestModel
{
    /// <summary>Unique key; scheduling the same key again moves its due time.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Seconds from now until the item is due.</summary>
    public int InSeconds { get; set; } = 10;

    /// <summary>Text carried by the event.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>One scheduled or fired item.</summary>
/// <param name="Key">The key.</param>
/// <param name="DueAt">When it is or was due.</param>
/// <param name="Message">The event text.</param>
/// <param name="FiredAt">When the runner handled it, or null while pending.</param>
public sealed record ScheduleEntryModel(string Key, DateTimeOffset DueAt, string Message, DateTimeOffset? FiredAt);

/// <summary>The pending and fired items.</summary>
/// <param name="Pending">Items not yet due.</param>
/// <param name="Fired">Items the runner handled, newest first.</param>
public sealed record ScheduleStateModel(IReadOnlyList<ScheduleEntryModel> Pending, IReadOnlyList<ScheduleEntryModel> Fired);
