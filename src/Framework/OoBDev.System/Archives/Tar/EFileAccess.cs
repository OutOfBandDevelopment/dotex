using System;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Specifies the access rights for a file or device (Windows API GENERIC access rights).
/// </summary>
[Flags]
public enum EFileAccess : uint
{
    /// <summary>
    /// Read access.
    /// </summary>
    GenericRead = 0x80000000,

    /// <summary>
    /// Write access.
    /// </summary>
    GenericWrite = 0x40000000,

    /// <summary>
    /// Execute access.
    /// </summary>
    GenericExecute = 0x20000000,

    /// <summary>
    /// All possible access rights.
    /// </summary>
    GenericAll = 0x10000000,
}
