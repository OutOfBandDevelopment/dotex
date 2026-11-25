using System.Collections.Generic;

namespace OoBDev.IO.Inventory;

/// <summary>Files with identical content, confirmed by SHA-512.</summary>
/// <param name="Length">Size in bytes.</param>
/// <param name="Sha512">The confirmed hash.</param>
/// <param name="Paths">Two or more paths.</param>
public sealed record DuplicateGroup(long Length, string Sha512, IReadOnlyList<string> Paths);
