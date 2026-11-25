using System;

namespace OoBDev.History;

/// <summary>The points of one series that fall in one tier bucket.</summary>
/// <param name="Start">Start of the bucket (UTC, aligned to the bucket length from the Unix epoch).</param>
/// <param name="Count">Number of points.</param>
/// <param name="Minimum">Smallest value.</param>
/// <param name="Maximum">Largest value.</param>
/// <param name="Sum">Sum of the values.</param>
/// <param name="Last">The value of the newest point by timestamp.</param>
public sealed record HistoryBucket(DateTimeOffset Start, long Count, double Minimum, double Maximum, double Sum, double Last)
{
    public double Average => Count == 0 ? 0 : Sum / Count;
}
