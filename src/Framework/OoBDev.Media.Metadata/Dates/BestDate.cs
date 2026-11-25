using System;

namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// The date chosen from the evidence of one file.
/// </summary>
/// <param name="Date">The winning date.</param>
/// <param name="Precision">The precision of the winning evidence.</param>
/// <param name="Source">The source of the winning evidence.</param>
/// <param name="Confidence">The combined confidence, at most 0.99.</param>
/// <param name="Sources">Every source that gave evidence, sorted and comma separated.</param>
/// <param name="Agreeing">How many other pieces of evidence agree with the winner.</param>
/// <param name="Conflict">True when two confident sources are more than a year apart.</param>
public sealed record BestDate(DateOnly Date, DatePrecision Precision, string Source, double Confidence, string Sources, int Agreeing, bool Conflict);
