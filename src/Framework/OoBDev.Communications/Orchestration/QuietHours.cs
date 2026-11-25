using System;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Orchestration;

/// <summary>Quiet-hour arithmetic in the target's own time zone; the window may cross midnight.</summary>
public static class QuietHours
{
    /// <summary>Gets whether <paramref name="now"/> falls inside the preference's quiet window; the end is exclusive.</summary>
    public static bool IsQuiet(TargetPreference preference, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(preference);
        if (preference.QuietHoursStart is not { } start || preference.QuietHoursEnd is not { } end || start == end)
        {
            return false;
        }

        var local = TimeOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, preference.TimeZone).DateTime);
        return start > end ? local >= start || local < end : local >= start && local < end;
    }

    /// <summary>Gets the first moment at or after <paramref name="now"/> that is the end of the window, or <paramref name="now"/> when the preference has no window.</summary>
    public static DateTimeOffset NextEnd(TargetPreference preference, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(preference);
        if (preference.QuietHoursEnd is not { } end)
        {
            return now;
        }

        var zone = preference.TimeZone;
        var local = TimeZoneInfo.ConvertTime(now, zone).DateTime;
        var candidate = local.Date + end.ToTimeSpan();
        if (candidate <= local)
        {
            candidate = candidate.AddDays(1);
        }

        if (zone.IsInvalidTime(candidate))
        {
            candidate = candidate.AddHours(1);
        }

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(candidate, zone), TimeSpan.Zero);
    }
}
