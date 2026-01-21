using System;

namespace OoBDev.System;

/// <summary>
/// Provides extension methods for DateTime operations including Unix timestamp conversion and date calculations.
/// </summary>
public static class DateTimeEx
{
    /// <summary>
    /// Converts a Unix timestamp (seconds since epoch) to a local DateTime.
    /// </summary>
    /// <param name="unixTimeStamp">The Unix timestamp to convert.</param>
    /// <returns>The DateTime value in local time.</returns>
    public static DateTime UnixTimeStampToLocalDateTime(this long unixTimeStamp)
    {
        // http://stackoverflow.com/questions/249760/how-to-convert-unix-timestamp-to-datetime-and-vice-versa 
        // Unix timestamp is seconds past epoch
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    /// <summary>
    /// Converts a local DateTime to a Unix timestamp (seconds since epoch).
    /// </summary>
    /// <param name="time">The DateTime to convert.</param>
    /// <returns>The Unix timestamp value.</returns>
    public static long LocalDateTimeToUnixTimeStamp(this DateTime time)
    {
        var utc = time.ToUniversalTime();
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        var result = utc.Subtract(dtDateTime);
        return (long)result.TotalSeconds;
    }

    /// <summary>
    /// Calculates the difference in months between two dates.
    /// </summary>
    /// <param name="ldate">The first date.</param>
    /// <param name="rdate">The second date.</param>
    /// <returns>The number of months between the dates.</returns>
    public static int MonthsDifferent(this DateTime ldate, DateTime rdate) => ldate.Month - rdate.Month + 12 * (ldate.Year - rdate.Year);

    /// <summary>
    /// Calculates the difference in months between two nullable dates.
    /// </summary>
    /// <param name="ldate">The first date (nullable).</param>
    /// <param name="rdate">The second date (nullable).</param>
    /// <returns>The number of months between the dates, or null if either date is null.</returns>
    public static int? MonthsDifferent(this DateTime? ldate, DateTime? rdate) => ldate?.Month - rdate?.Month + 12 * (ldate?.Year - rdate?.Year);

    /// <summary>
    /// Calculates the difference in days between two dates using the 360-day year convention (12 months of 30 days each).
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>The number of days between the dates using the 360-day convention.</returns>
    public static int Days360(this DateTime startDate, DateTime endDate)
    {
        var startDay = startDate.Day;
        var startMonth = startDate.Month;
        var startYear = startDate.Year;
        var endDay = endDate.Day;
        var endMonth = endDate.Month;
        var endYear = endDate.Year;

        if (startDay == 31 || startDate.IsLastDayOfFebruary())
            startDay = 30;

        if (startDay == 30 && endDay == 31)
            endDay = 30;

        return (endYear - startYear) * 360 + (endMonth - startMonth) * 30 + (endDay - startDay);
    }

    /// <summary>
    /// Determines whether the date is the last day of February.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is the last day of February; otherwise, false.</returns>
    public static bool IsLastDayOfFebruary(this DateTime date) => date.Month == 2 && date.Day == DateTime.DaysInMonth(date.Year, date.Month);
}
