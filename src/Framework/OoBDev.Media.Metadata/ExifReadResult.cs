using System.Collections.Generic;

namespace OoBDev.Media.Metadata;

/// <summary>The EXIF fields that could be read and a description of everything that was damaged.</summary>
/// <param name="Fields">The valid fields; damaged ones are null.</param>
/// <param name="Problems">One sentence per damaged or skipped item; empty when the data is sound or absent.</param>
public sealed record ExifReadResult(ExifFields Fields, IReadOnlyList<string> Problems);
