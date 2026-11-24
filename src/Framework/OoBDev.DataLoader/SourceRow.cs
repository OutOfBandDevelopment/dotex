using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>A row as read from a source.</summary>
/// <param name="Line">One-based position in the source.</param>
/// <param name="Values">Column name to value.</param>
public sealed record SourceRow(int Line, IReadOnlyDictionary<string, string?> Values);
