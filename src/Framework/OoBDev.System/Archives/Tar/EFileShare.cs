using System;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Specifies the sharing mode for file access (Windows API FILE_SHARE constants).
/// </summary>
[Flags]
public enum EFileShare : uint
{
    /// <summary>
    /// Prevents other processes from opening the file.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// Enables subsequent open operations on the file to request read access.
    /// </summary>
    Read = 0x00000001,

    /// <summary>
    /// Enables subsequent open operations on the file to request write access.
    /// </summary>
    Write = 0x00000002,

    /// <summary>
    /// Enables subsequent open operations on the file to request delete access.
    /// </summary>
    Delete = 0x00000004,
}
