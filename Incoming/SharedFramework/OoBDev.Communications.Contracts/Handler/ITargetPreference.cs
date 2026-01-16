using System;
using System.Globalization;

namespace OoBDev.Communications.Handler
{
    //TODO: consider making these getter only
    public interface ITargetPreference
    {
        string[]? Channels { get; set; }
        CultureInfo? Culture { get; set; }
        TimeSpan? EndTime { get; set; }
        bool SkipWeekends { get; set; }
        TimeSpan? StartTime { get; set; }
        TimeSpan? TimeZone { get; set; }
    }
}