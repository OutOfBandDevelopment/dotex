using System;

namespace OoBDev.System.IO.Fat;

/// <summary>
/// The attribute byte of a FAT directory entry.
/// </summary>
[Flags]
public enum FatAttributes : byte
{
    /// <summary>No attribute.</summary>
    None = 0,

    /// <summary>The file is read only.</summary>
    ReadOnly = 0x01,

    /// <summary>The entry is hidden.</summary>
    Hidden = 0x02,

    /// <summary>The entry belongs to the operating system.</summary>
    System = 0x04,

    /// <summary>The entry is the volume label, not a file.</summary>
    VolumeLabel = 0x08,

    /// <summary>The entry is a directory.</summary>
    Directory = 0x10,

    /// <summary>The file changed since the last backup.</summary>
    Archive = 0x20,
}
