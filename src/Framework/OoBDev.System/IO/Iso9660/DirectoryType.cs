using System;

namespace OoBDev.System.IO.Iso9660;

/// <summary>
/// Defines flags for ISO 9660 directory record attributes.
/// These flags indicate file/directory properties such as visibility, type, and metadata.
/// </summary>
[Flags]
public enum DirectoryType : byte
{
    /// <summary>
    /// Indicates a hidden file (bit 0).
    /// </summary>
    Hidden = 0x01,

    /// <summary>
    /// Indicates a directory rather than a file (bit 1).
    /// </summary>
    Directory = 0x02,

    /// <summary>
    /// Indicates an associated file (bit 2).
    /// </summary>
    AssociatedFile = 0x04,

    /// <summary>
    /// Indicates record format is specified (bit 3).
    /// </summary>
    RecordFormat = 0x08,

    /// <summary>
    /// Indicates permissions are specified (bit 4).
    /// </summary>
    Permission = 0x10,

    /// <summary>
    /// Reserved flag 1 (bit 5).
    /// </summary>
    Reserved1 = 0x20,

    /// <summary>
    /// Reserved flag 2 (bit 6).
    /// </summary>
    Reserved2 = 0x40,

    /// <summary>
    /// Indicates this is not the final record for the file (bit 7).
    /// </summary>
    FinalRecord = 0x80
}
