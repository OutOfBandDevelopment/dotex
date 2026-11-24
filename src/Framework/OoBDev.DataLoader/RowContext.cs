using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>A row moving through the pipeline; handlers may change <see cref="Values"/>.</summary>
/// <param name="Line">One-based position in the source.</param>
/// <param name="Values">Column name to value.</param>
public sealed record RowContext(int Line, IDictionary<string, string?> Values);
