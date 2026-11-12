using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// set of commands for AppleSoft DOS 3.3 disk images.
/// </summary>
public interface IDiskImageCommands
{
    /// <summary>
    /// Returns list of catalog entries with files
    /// </summary>
    /// <param name="diskImage"></param>
    /// <returns></returns>
    IEnumerable<CatalogEntry> GetCatalogs(Stream diskImage);

    /// <summary>
    /// Retrieve Volume table of contents.
    /// </summary>
    /// <param name="diskImage"></param>
    /// <returns></returns>
    VolumeTableOfContents GetVolumeTableOfContents(Stream diskImage);

    /// <summary>
    /// Returns list of populated track/sectors for given file
    /// </summary>
    /// <param name="diskImage"></param>
    /// <param name="fileEntry"></param>
    /// <returns></returns>
    IEnumerable<TrackSectorList> GetTrackSectorListForFileEntry(Stream diskImage, FileEntry fileEntry);

    /// <summary>
    /// retrieve binary data for input file
    /// </summary>
    /// <param name="diskImage"></param>
    /// <param name="fileEntry"></param>
    /// <returns></returns>
    Span<byte> GetDataFileEntry(Stream diskImage, FileEntry fileEntry);

    /// <summary>
    /// Retrieves the content of a file with its real length. Binary files carry a 4-byte header (load address, length) and
    /// Applesoft and Integer BASIC files a 2-byte length prefix; the header is dropped. Text files end before the first zero byte.
    /// Other types are returned whole sectors, as <see cref="GetDataFileEntry"/> does.
    /// </summary>
    /// <param name="diskImage">The disk image.</param>
    /// <param name="fileEntry">The catalog entry.</param>
    /// <returns>The file content; a header that claims more than the sectors hold is clipped to what is there.</returns>
    byte[] GetFileContent(Stream diskImage, FileEntry fileEntry);
}