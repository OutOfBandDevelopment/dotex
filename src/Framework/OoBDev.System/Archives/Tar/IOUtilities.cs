using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Provides I/O utility methods for working with files, including support for long file paths.
/// </summary>
public static class IOUtilities
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern SafeFileHandle CreateFile(
        string lpFileName,
        EFileAccess dwDesiredAccess,
        EFileShare dwShareMode,
        nint lpSecurityAttributes,
        ECreationDisposition dwCreationDisposition,
        EFileAttributes dwFlagsAndAttributes,
        nint hTemplateFile);

    internal static nint INVALID_HANDLE_VALUE = new(-1);
    internal static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
    internal const int MAX_PATH = 260;

    /// <summary>
    /// Opens a file stream with support for file paths longer than MAX_PATH (260 characters).
    /// </summary>
    /// <param name="fileName">The name of the file to open.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="fileAccess">The file access.</param>
    /// <param name="fileShare">The file sharing mode.</param>
    /// <returns>A FileStream for the opened file.</returns>
    public static FileStream OpenFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
    {
        if (fileName.Length <= 260)
            return new FileStream(fileName, fileMode, fileAccess);

        var handle = CreateFile(
            @"\\?\" + fileName,
            fileAccess.Convert(),
            fileShare.Convert(),
            nint.Zero,
            fileMode.Convert(),
            0, nint.Zero);
        var stream = new FileStream(handle, fileAccess);
        if (fileMode == FileMode.Append)
            stream.Seek(0, SeekOrigin.End);

        return stream;
    }

    internal static EFileAccess Convert(this FileAccess fileAccess) => fileAccess switch
    {
        FileAccess.Read => EFileAccess.GenericRead,
        FileAccess.ReadWrite => EFileAccess.GenericRead | EFileAccess.GenericRead,
        FileAccess.Write => EFileAccess.GenericWrite,
        _ => throw new NotSupportedException(),
    };

    internal static EFileShare Convert(this FileShare fileShare) => fileShare switch
    {
        FileShare.Delete => EFileShare.Delete,
        FileShare.Read => EFileShare.Read,
        FileShare.ReadWrite => EFileShare.Write | EFileShare.Read,
        FileShare.Write => EFileShare.Write,
        FileShare.None => EFileShare.None,
        _ => throw new NotSupportedException(),
    };

    internal static ECreationDisposition Convert(this FileMode fileMode) => fileMode switch
    {
        FileMode.Append => ECreationDisposition.OpenAlways,
        FileMode.Create => ECreationDisposition.CreateAlways,
        FileMode.CreateNew => ECreationDisposition.New,
        FileMode.Open => ECreationDisposition.OpenExisting,
        FileMode.OpenOrCreate => ECreationDisposition.OpenAlways,
        FileMode.Truncate => ECreationDisposition.TruncateExisting,
        _ => throw new NotSupportedException(),
    };
}
