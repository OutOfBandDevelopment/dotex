using System;
using System.IO;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// Adds, replaces and deletes files in an Apple DOS 3.3 disk image (143,360 bytes, DOS sector order).
/// </summary>
public interface IDiskImageWriter
{
    /// <summary>
    /// Stores a file: allocates a track/sector list and data sectors, takes a free catalog slot and updates the sector bitmap.
    /// The image is written back only when everything fits, so a failure leaves the stream unchanged.
    /// </summary>
    /// <param name="diskImage">A readable, writable, seekable stream over the image.</param>
    /// <param name="name">The file name: 1 to 30 printable ASCII characters, no comma; stored upper-case.</param>
    /// <param name="fileType">The DOS file type, including the locked flag.</param>
    /// <param name="data">The bytes stored as given, zero-padded to a whole sector. For Applesoft and binary files the caller supplies DOS's own length (and load address) prefix.</param>
    /// <param name="replace"><see langword="true"/> to replace a file of the same name; otherwise an existing name is an error.</param>
    /// <exception cref="InvalidOperationException">The name exists and <paramref name="replace"/> is false, the catalog is full, or the disk has too few free sectors.</exception>
    /// <exception cref="InvalidDataException">The stream is not a DOS 3.3 image or its catalog is damaged.</exception>
    /// <exception cref="ArgumentException">The name is not valid.</exception>
    void AddFile(Stream diskImage, string name, AppleFileType fileType, ReadOnlySpan<byte> data, bool replace = false);

    /// <summary>
    /// Deletes a file: frees its sectors in the bitmap and marks the catalog entry deleted the way DOS does (first byte <c>$FF</c>, original track kept in the last name byte).
    /// </summary>
    /// <param name="diskImage">A readable, writable, seekable stream over the image.</param>
    /// <param name="name">The file name.</param>
    /// <returns><see langword="true"/> when the file existed and was deleted.</returns>
    /// <exception cref="InvalidDataException">The stream is not a DOS 3.3 image or its catalog is damaged.</exception>
    bool DeleteFile(Stream diskImage, string name);
}
