using System;
using System.Globalization;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface IDeliveryPreference
    {
        string[] Channels { get; }
        CultureInfo Culture { get; }
        TimeSpan? EndTime { get; }
        bool SkipWeekends { get; }
        TimeSpan? StartTime { get; }
        TimeSpan? TimeZone { get; }
    }
}