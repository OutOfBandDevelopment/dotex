using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>A row that did not load.</summary>
/// <param name="Line">Source line.</param>
/// <param name="Message">Reason.</param>
public sealed record RowError(int Line, string Message);
