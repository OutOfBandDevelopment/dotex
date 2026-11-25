using System;

namespace OoBDev.History;

/// <summary>One measurement of a series.</summary>
public readonly record struct HistoryPoint(DateTimeOffset Timestamp, double Value);
