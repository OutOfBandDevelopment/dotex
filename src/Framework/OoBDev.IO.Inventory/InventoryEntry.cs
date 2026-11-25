using System;

namespace OoBDev.IO.Inventory;

/// <summary>One scanned file.</summary>
/// <param name="Path">Full path.</param>
/// <param name="Length">Size in bytes.</param>
/// <param name="Modified">Last write time (UTC).</param>
/// <param name="FastHash">Hex XxHash3 of the content.</param>
/// <param name="Sha512">Hex SHA-512, set once a duplicate candidate was confirmed.</param>
public sealed record InventoryEntry(string Path, long Length, DateTimeOffset Modified, string FastHash, string? Sha512 = null);
