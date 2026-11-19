using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// Chooses a file's date from the evidence: the most confident source wins, ties go to the finer precision, and each source that agrees raises the combined
/// confidence (1 minus the product of the doubts, capped at 0.99).
/// </summary>
public static class DateCombiner
{
    /// <summary>Confidence of a plausible EXIF capture time.</summary>
    public const double ExifConfidence = 0.95;

    /// <summary>Confidence of an EXIF capture time with an implausible year.</summary>
    public const double BadExifConfidence = 0.05;

    /// <summary>Confidence of a file system time.</summary>
    public const double FileTimeConfidence = 0.30;

    /// <summary>Confidence of a file system time that <see cref="BulkFiles"/> or more files in a folder share (a copy or sync, not a capture).</summary>
    public const double FileTimeBulkConfidence = 0.15;

    /// <summary>Confidence of a file system time before 1995.</summary>
    public const double FileTimeEpochConfidence = 0.03;

    /// <summary>How many files in a folder sharing one file time make that time a bulk time.</summary>
    public const int BulkFiles = 20;

    /// <summary>Whether two dates agree at the coarser of their precisions (days within one, months equal, or years equal).</summary>
    /// <param name="first">The first date.</param>
    /// <param name="firstPrecision">The first precision.</param>
    /// <param name="second">The second date.</param>
    /// <param name="secondPrecision">The second precision.</param>
    /// <returns>True when they agree.</returns>
    public static bool Agree(DateOnly first, DatePrecision firstPrecision, DateOnly second, DatePrecision secondPrecision)
    {
        var coarse = (DatePrecision)Math.Max((int)firstPrecision, (int)secondPrecision);
        return coarse switch
        {
            DatePrecision.Year => first.Year == second.Year,
            DatePrecision.Month => (first.Year, first.Month) == (second.Year, second.Month),
            _ => Math.Abs(first.DayNumber - second.DayNumber) <= 1,
        };
    }

    /// <summary>Combines the evidence of one file.</summary>
    /// <param name="evidence">At least one piece of evidence.</param>
    /// <returns>The chosen date.</returns>
    public static BestDate Combine(IReadOnlyList<DateEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidence.Count == 0)
            throw new ArgumentException("At least one piece of evidence is needed.", nameof(evidence));
        var ordered = evidence.Select((e, i) => (e, i)).OrderByDescending(t => t.e.Confidence).ThenBy(t => (int)t.e.Precision).ThenBy(t => t.i).Select(t => t.e).ToList();
        var top = ordered[0];
        var doubt = 1 - top.Confidence;
        var agreeing = 0;
        var conflict = false;
        foreach (var e in ordered.Skip(1))
        {
            if (Agree(top.Date, top.Precision, e.Date, e.Precision))
            {
                doubt *= 1 - e.Confidence;
                agreeing++;
            }
            else if (e.Confidence >= 0.4 && top.Confidence >= 0.4 && Math.Abs(top.Date.DayNumber - e.Date.DayNumber) > 366)
            {
                conflict = true;
            }
        }

        var sources = string.Join(',', ordered.Select(e => e.Source).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        return new BestDate(top.Date, top.Precision, top.Source, Math.Round(Math.Min(0.99, 1 - doubt), 2, MidpointRounding.ToEven), sources, agreeing, conflict);
    }
}
