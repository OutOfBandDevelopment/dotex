using OoBDev.Communications.Handler;
using System;
using System.Globalization;

namespace OoBDev.Communications.Abstractions.Models
{
    public class TargetPreferenceModel : ITargetPreference
    {
        public string[]? Channels { get; set; }
        public CultureInfo? Culture { get; set; }
        public bool SkipWeekends { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan? TimeZone { get; set; }
    }
}