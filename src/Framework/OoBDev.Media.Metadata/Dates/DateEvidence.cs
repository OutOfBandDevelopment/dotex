using System;

namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// One source's claim about a file's date.
/// </summary>
/// <param name="Source">The source name, for example <c>exif</c>, <c>path</c> or <c>fs</c>.</param>
/// <param name="Date">The claimed date.</param>
/// <param name="Precision">How exactly the source knows it.</param>
/// <param name="Confidence">How much the source is trusted, 0 to 1.</param>
public sealed record DateEvidence(string Source, DateOnly Date, DatePrecision Precision, double Confidence);
