using System;

namespace OoBDev.System.IO.Fat;

/// <summary>
/// A file or directory found in a FAT directory.
/// </summary>
/// <param name="Name">The long name when the entry has a valid one, otherwise the short name.</param>
/// <param name="ShortName">The 8.3 name.</param>
/// <param name="Attributes">The attribute byte.</param>
/// <param name="Size">The file size in bytes; 0 for a directory.</param>
/// <param name="FirstCluster">The first cluster of the content; 0 for an empty file (and for a parent entry that points at the root).</param>
/// <param name="Created">The creation time, or <see langword="null"/> when not stored or not a valid date.</param>
/// <param name="Modified">The last write time, or <see langword="null"/> when not a valid date.</param>
/// <param name="Accessed">The last access date, or <see langword="null"/> when not stored or not a valid date.</param>
public sealed record FatDirectoryEntry(
    string Name,
    string ShortName,
    FatAttributes Attributes,
    long Size,
    uint FirstCluster,
    DateTime? Created,
    DateTime? Modified,
    DateTime? Accessed)
{
    /// <summary>Gets a value indicating whether the entry is a directory.</summary>
    public bool IsDirectory => (Attributes & FatAttributes.Directory) != 0;
}
