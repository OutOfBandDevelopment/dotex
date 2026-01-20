using System;
using System.Collections.Generic;

namespace OoBDev.ComplexEvents.Abstractions.Schedulers
{
    public interface IScheduleCalculations
    {
        DateTimeOffset? GetNextOccurrence(IEnumerable<string>? schedules);
        DateTimeOffset? GetNextOccurrence(params string[]? schedules);
        DateTimeOffset? GetNextOccurrence(string schedule, params string[]? schedules);
    }
}
