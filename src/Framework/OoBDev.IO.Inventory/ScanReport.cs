using System.Collections.Generic;

namespace OoBDev.IO.Inventory;

/// <summary>Outcome of one scan.</summary>
/// <param name="Scanned">Files seen.</param>
/// <param name="Hashed">Files whose content was read (new or changed).</param>
/// <param name="Skipped">Unreadable files, as path and reason.</param>
public sealed record ScanReport(int Scanned, int Hashed, IReadOnlyList<(string Path, string Reason)> Skipped);
