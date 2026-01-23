using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OoBDev.System.IO.Iso9660;

/// <summary>
/// Represents a directory record in an ISO 9660 file system.
/// Contains metadata about a file or directory including location, size, timestamps, and attributes.
/// </summary>
public class DirectoryRecord : IEnumerable<DirectoryRecord>
{
    /// <summary>
    /// Initializes a new instance of the DirectoryRecord class by parsing raw ISO 9660 directory record data.
    /// </summary>
    /// <param name="buffer">The byte array containing the directory record data.</param>
    /// <param name="offset">The starting offset in the buffer where the directory record begins.</param>
    /// <param name="file">The ISO 9660 disc stream for reading file data.</param>
    /// <param name="parent">The parent directory record, or null if this is the root.</param>
    internal DirectoryRecord(byte[] buffer,
                             int offset,
                             Stream? file,
                             DirectoryRecord? parent)
    {
        disc = file;
        Parent = parent;

        //1	22 
        BytesInRecord = buffer[offset];
        offset++;

        //1	00 
        SectorsInExtended = buffer[offset];
        offset++;

        //8	1B 00 00 00 - 00 00 00 1B 
        FirstSector = buffer.GetUInt32(ref offset, 8);

        //8	00 08 00 00 - 00 00 08 00 
        Size = buffer.GetUInt32(ref offset, 8);

        //1	63 
        var yearOffset = buffer[offset];
        offset++;
        //1	0B 
        var month = buffer[offset];
        offset++;
        //1	18 
        var day = buffer[offset];
        offset++;
        //1	0F 
        var hour = buffer[offset];
        offset++;
        //1	35 
        var minute = buffer[offset];
        offset++;
        //1	00 
        var second = buffer[offset];
        offset++;
        //1	00 
        var quaterHourOffset = (sbyte)buffer[offset];
        offset++;

        var timeOffset = quaterHourOffset * 15d;
        DateTime = new DateTime(yearOffset + 1900,
                                     month == 0 ? 1 : month,
                                     day == 0 ? 1 : month,
                                     hour,
                                     minute,
                                     second
                                     ).AddMinutes(timeOffset);

        //1	02 
        DirectoryType = (DirectoryType)buffer[offset];
        offset++;

        //1	00 
        FileUnitSize = buffer[offset];
        offset++;

        //1	00 
        InterleaveGapSize = buffer[offset];
        offset++;

        //4	01 00 - 00 01 
        VolumeSequenceNumber = buffer.GetUInt16(ref offset, 4);

        //1	01 
        IdentifierLength = buffer[offset];
        offset++;

        Identifier = buffer.GetString(ref offset,
                                           IdentifierLength,
                                           Encoding.ASCII);
        if (string.IsNullOrEmpty(Identifier))
            Identifier = ".";
        else if (Identifier == "\x01")
            Identifier = "..";

        //    00 
    }

    #region Properties

    /// <summary>
    /// Gets the number of bytes in the directory record (must be even).
    /// </summary>
    public byte BytesInRecord { get; init; }

    /// <summary>
    /// Gets the number of sectors in the extended attribute record (typically 0).
    /// </summary>
    public byte SectorsInExtended { get; init; }

    /// <summary>
    /// Gets the number of the first sector of file data or directory (zero for empty files).
    /// Stored as a both-endian double word in ISO 9660.
    /// </summary>
    public uint FirstSector { get; init; }

    /// <summary>
    /// Gets the number of bytes of file data or length of directory, excluding the extended attribute record.
    /// Stored as a both-endian double word in ISO 9660.
    /// </summary>
    public uint Size { get; init; }

    /// <summary>
    /// Gets the date and time of the file or directory, adjusted for GMT offset.
    /// </summary>
    public DateTime DateTime { get; init; }

    /// <summary>
    /// Gets the directory type flags indicating attributes such as hidden, directory, associated file, etc.
    /// </summary>
    public DirectoryType DirectoryType { get; init; }

    /// <summary>
    /// Gets the file unit size for interleaved files (typically 0).
    /// </summary>
    public byte FileUnitSize { get; init; }

    /// <summary>
    /// Gets the interleave gap size for interleaved files (typically 0).
    /// </summary>
    public byte InterleaveGapSize { get; init; }

    /// <summary>
    /// Gets the volume sequence number (typically 1).
    /// </summary>
    public ushort VolumeSequenceNumber { get; init; }

    /// <summary>
    /// Gets the length of the identifier string.
    /// </summary>
    public byte IdentifierLength { get; init; }

    /// <summary>
    /// Gets the file or directory name identifier.
    /// "." represents the current directory, ".." represents the parent directory.
    /// </summary>
    public string Identifier { get; init; }

    #endregion

    /// <summary>
    /// Returns a string representation of the directory record showing the identifier and type.
    /// </summary>
    /// <returns>A string in the format "Identifier - DirectoryType".</returns>
    public override string ToString() => $"{Identifier} - {DirectoryType}";

    private IEnumerable<DirectoryRecord> GetChildren()
    {
        if (IsDirectory)
        {
            var sector = new byte[2048];
            var bufferLen = 0;

            if (disc != null)
                lock (disc)
                {
                    disc.Seek(FirstSector * Size,
                            SeekOrigin.Begin);
                    bufferLen = disc.Read(sector, 0, sector.Length);

                }
            for (var i = 0; i < bufferLen;)
            {
                var directorRecord = new DirectoryRecord(sector,
                                                         i,
                                                         disc,
                                                         this);
                if (directorRecord.BytesInRecord < 34)
                    break;
                i += directorRecord.BytesInRecord;
                yield return directorRecord;
            }
        }
    }
    private byte[]? GetBuffer()
    {
        if (disc == null) return null;
        lock (disc)
        {
            disc.Seek(FirstSector * 2048, SeekOrigin.Begin);
            var buffer = new byte[Size];
            var bufferLen = disc.Read(buffer, 0, (int)Size);
            return buffer;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Stream? disc;

    /// <summary>
    /// Gets the parent directory record, or null if this is the root directory.
    /// </summary>
    public DirectoryRecord? Parent { get; init; }

    /// <summary>
    /// Gets the root directory record by traversing up the parent chain.
    /// </summary>
    [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public DirectoryRecord Root => field ??= Parent?.Root ?? this;

    /// <summary>
    /// Gets a value indicating whether this record represents a directory.
    /// </summary>
    public bool IsDirectory => (DirectoryType & DirectoryType.Directory) != 0;

    /// <summary>
    /// Gets the child directory records if this is a directory.
    /// </summary>
    public IEnumerable<DirectoryRecord> Children
    {
        get
        {
            if (disc != null && IsDirectory)
                foreach (var item in GetChildren())
                    yield return item;
        }
    }

    /// <summary>
    /// Gets the raw file data as a byte array, or null if this is a directory or no disc is available.
    /// </summary>
    public byte[]? Data => disc switch { null => null, _ => GetBuffer() };

    /// <summary>
    /// Gets the file data encoded as a Base64 string, or null if no data is available.
    /// </summary>
    public string? DataBase64 => Data switch { null => null, byte[] data => Convert.ToBase64String(data) };

    #region IEnumerable<DirectoryRecord> Members

    /// <summary>
    /// Returns an enumerator that iterates through the child directory records.
    /// </summary>
    /// <returns>An enumerator for the children collection.</returns>
    public IEnumerator<DirectoryRecord> GetEnumerator() =>
        (Children ?? []).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
