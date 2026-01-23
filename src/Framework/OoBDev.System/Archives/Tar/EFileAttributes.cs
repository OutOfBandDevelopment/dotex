using System;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// File attributes and flags for Windows file operations (CreateFile dwFlagsAndAttributes parameter).
/// </summary>
[Flags]
public enum EFileAttributes : uint
{
    /// <summary>
    /// The file is read-only.
    /// </summary>
    Readonly = 0x00000001,

    /// <summary>
    /// The file is hidden.
    /// </summary>
    Hidden = 0x00000002,

    /// <summary>
    /// The file is a system file.
    /// </summary>
    System = 0x00000004,

    /// <summary>
    /// The handle identifies a directory.
    /// </summary>
    Directory = 0x00000010,

    /// <summary>
    /// The file should be archived (backup applications use this attribute).
    /// </summary>
    Archive = 0x00000020,

    /// <summary>
    /// Reserved for system use.
    /// </summary>
    Device = 0x00000040,

    /// <summary>
    /// The file has no other attributes set (this attribute is valid only when used alone).
    /// </summary>
    Normal = 0x00000080,

    /// <summary>
    /// The file is being used for temporary storage.
    /// </summary>
    Temporary = 0x00000100,

    /// <summary>
    /// The file is a sparse file.
    /// </summary>
    SparseFile = 0x00000200,

    /// <summary>
    /// The file or directory has an associated reparse point.
    /// </summary>
    ReparsePoint = 0x00000400,

    /// <summary>
    /// The file or directory is compressed.
    /// </summary>
    Compressed = 0x00000800,

    /// <summary>
    /// The data of the file is not immediately available (file is offline).
    /// </summary>
    Offline = 0x00001000,

    /// <summary>
    /// The file will not be indexed by the content indexing service.
    /// </summary>
    NotContentIndexed = 0x00002000,

    /// <summary>
    /// The file or directory is encrypted.
    /// </summary>
    Encrypted = 0x00004000,

    /// <summary>
    /// Write operations will not go through any intermediate cache, they will go directly to disk.
    /// </summary>
    Write_Through = 0x80000000,

    /// <summary>
    /// The file is being opened or created for asynchronous I/O.
    /// </summary>
    Overlapped = 0x40000000,

    /// <summary>
    /// The file is being opened with no system caching for data reads and writes.
    /// </summary>
    NoBuffering = 0x20000000,

    /// <summary>
    /// Access is intended to be random (the system can use this as a hint to optimize file caching).
    /// </summary>
    RandomAccess = 0x10000000,

    /// <summary>
    /// Access is intended to be sequential from beginning to end.
    /// </summary>
    SequentialScan = 0x08000000,

    /// <summary>
    /// The file is to be deleted immediately after all of its handles are closed.
    /// </summary>
    DeleteOnClose = 0x04000000,

    /// <summary>
    /// The file is being opened or created for a backup or restore operation.
    /// </summary>
    BackupSemantics = 0x02000000,

    /// <summary>
    /// Access will occur according to POSIX rules (case-sensitive, etc.).
    /// </summary>
    PosixSemantics = 0x01000000,

    /// <summary>
    /// The file being opened is a reparse point and should not be followed.
    /// </summary>
    OpenReparsePoint = 0x00200000,

    /// <summary>
    /// The file data is requested, but it should continue to be located in remote storage.
    /// </summary>
    OpenNoRecall = 0x00100000,

    /// <summary>
    /// The same pipe name cannot be opened by another CreateFile call.
    /// </summary>
    FirstPipeInstance = 0x00080000
}
