using System;

namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// A date read from a path.
/// </summary>
/// <param name="Date">The date.</param>
/// <param name="Precision">Day, month or year.</param>
/// <param name="Matched">The text that matched.</param>
/// <param name="Confidence">0.75 (day), 0.60 (month) or 0.45 (year), plus 0.05 in the file name.</param>
/// <param name="Detail">A sentence saying where the date was found.</param>
public sealed record PathDate(DateOnly Date, DatePrecision Precision, string Matched, double Confidence, string Detail);
