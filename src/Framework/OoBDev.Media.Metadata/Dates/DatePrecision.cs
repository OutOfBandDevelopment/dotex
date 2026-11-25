namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// How exactly a date is known. The order is finest first.
/// </summary>
public enum DatePrecision
{
    /// <summary>Year, month and day are known.</summary>
    Day = 0,

    /// <summary>Only year and month are known (the day is 1).</summary>
    Month = 1,

    /// <summary>Only the year is known (month and day are 1).</summary>
    Year = 2,
}
