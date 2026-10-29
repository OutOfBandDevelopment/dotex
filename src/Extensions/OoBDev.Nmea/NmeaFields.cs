using System;
using System.Globalization;

namespace OoBDev.Nmea;

internal static class NmeaFields
{
    public static double? Double(string value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static int? Int(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) ? result : null;

    // ddmm.mmmm or dddmm.mmmm plus a hemisphere letter to signed decimal degrees
    public static double? Degrees(string value, string hemisphere)
    {
        if (Double(value) is not { } raw) return null;

        var degrees = Math.Truncate(raw / 100.0);
        var decimalDegrees = degrees + ((raw - (degrees * 100.0)) / 60.0);
        return hemisphere is "S" or "W" ? -decimalDegrees : decimalDegrees;
    }

    // hhmmss[.ss]
    public static TimeOnly? Time(string value)
    {
        if (value.Length < 6
            || !int.TryParse(value.AsSpan(0, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var hour)
            || !int.TryParse(value.AsSpan(2, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var minute)
            || !int.TryParse(value.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var second)
            || hour > 23 || minute > 59 || second > 59)
            return null;

        return new TimeOnly(hour, minute, second);
    }

    // ddmmyy, read as the 21st century
    public static DateOnly? Date(string value)
    {
        if (value.Length != 6
            || !int.TryParse(value.AsSpan(0, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var day)
            || !int.TryParse(value.AsSpan(2, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || !int.TryParse(value.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(2000 + year, month))
            return null;

        return new DateOnly(2000 + year, month, day);
    }
}
