using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OoBDev.System.IO.Iso9660;

/// <summary>
/// Represents the primary volume descriptor in an ISO 9660 file system.
/// Contains metadata about the volume including identifiers, sector information, timestamps, and the root directory.
/// </summary>
public class VolumeDescription : IEnumerable<DirectoryRecord>, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the VolumeDescription class by parsing ISO 9660 volume descriptor data.
    /// </summary>
    /// <param name="buffer">The byte array containing the volume descriptor sector data.</param>
    /// <param name="encoding">The text encoding used for string fields (typically ASCII).</param>
    /// <param name="reader">The ISO 9660 disc stream for reading directory and file data.</param>
    private VolumeDescription(byte[] buffer, Encoding encoding, Stream reader)
    {
        //  1      1
        var offset = 1;
        //  6      67, 68, 48, 48, 49 and 1, respectively (same as Volume
        //           Descriptor Set Terminator)
        DescriptorSet = buffer.GetString(ref offset, 6, encoding);
        //  1      0
        offset += 1;    //padding
        // 32      system identifier
        SystemIdentifier = buffer.GetString(ref offset, 32, encoding);
        // 32      volume identifier
        VolumeIdentifier = buffer.GetString(ref offset, 32, encoding);
        //  8      zeros
        offset += 8;    // padding
        //  8      total number of sectors, as a both endian double word
        SectorCount = buffer.GetUInt32(ref offset, 8);
        // 32      zeros
        offset += 32;   // padding
        //  4      1, as a both endian word [volume set size]
        VolumeSetSize = buffer.GetUInt16(ref offset, 4);
        //  4      1, as a both endian word [volume sequence number]
        VolumeSequenceNumber = buffer.GetUInt16(ref offset, 4);
        //  4      2048 (the sector size), as a both endian word
        SectorSize = buffer.GetUInt16(ref offset, 4);
        //  8      path table length in bytes, as a both endian double word
        PathTableLength = buffer.GetUInt32(ref offset, 8);
        //  4      number of first sector in first little endian path table,
        //           as a little endian double word
        var v1 = buffer.GetUInt32(ref offset, 4);
        //offset += 4;   // padding
        //  4      number of first sector in second little endian path table,
        //           as a little endian double word, or zero if there is no
        //           second little endian path table
        var v2 = buffer.GetUInt32(ref offset, 4);
        //offset += 4;   // padding
        //  4      number of first sector in first big endian path table,
        //           as a big endian double word
        FirstSectorFirst = buffer.GetUInt32(ref offset, 4);
        //  4      number of first sector in second big endian path table,
        //           as a big endian double word, or zero if there is no
        //           second big endian path table
        FirstSectorSecond = buffer.GetUInt32(ref offset, 4);
        // 34      root directory record, as described below
        var rootDir = new byte[34];
        Array.Copy(buffer, offset, rootDir, 0, 34);
        DirectoryRecord = new DirectoryRecord(rootDir, 0, reader, null);
        offset += 34;    // 4 big endian
        //128      volume set identifier
        VolumeSetIdentifier = buffer.GetString(ref offset, 128, encoding);
        //128      publisher identifier
        PublisherIdentifier = buffer.GetString(ref offset, 128, encoding);
        //128      data preparer identifier
        DataPreparerIdentifier = buffer.GetString(ref offset, 128, encoding);
        //128      application identifier
        ApplicationIdentifier = buffer.GetString(ref offset, 128, encoding);
        // 37      copyright file identifier
        CopyRightFileIdentifier = buffer.GetString(ref offset, 37, encoding);
        // 37      abstract file identifier
        AbstractFileIdentifier = buffer.GetString(ref offset, 37, encoding);
        // 37      bibliographical file identifier
        BibliographyFileIdentifier = buffer.GetString(ref offset, 37, encoding);
        // 17      date and time of volume creation
        VolumeCreation = buffer.GetDateTime(ref offset, 17);
        // 17      date and time of most recent modification
        VolumeModification = buffer.GetDateTime(ref offset, 17);
        // 17      date and time when volume expires
        VolumeExpires = buffer.GetDateTime(ref offset, 17);
        // 17      date and time when volume is effective
        VolumeEffective = buffer.GetDateTime(ref offset, 17);
        //  1      1
        //  1      0
        //512      reserved for application use (usually zeros)
        //653      zeros
    }

    /// <summary>
    /// Gets the descriptor set identifier (typically "CD001").
    /// </summary>
    public string DescriptorSet { get; init; }

    /// <summary>
    /// Gets the system identifier string.
    /// </summary>
    public string SystemIdentifier { get; init; }

    /// <summary>
    /// Gets the volume identifier (volume label/name).
    /// </summary>
    public string VolumeIdentifier { get; init; }

    /// <summary>
    /// Gets the total number of sectors in the volume.
    /// </summary>
    public uint SectorCount { get; init; }

    /// <summary>
    /// Gets the volume set size (typically 1).
    /// </summary>
    public ushort VolumeSetSize { get; init; }

    /// <summary>
    /// Gets the volume sequence number (typically 1).
    /// </summary>
    public ushort VolumeSequenceNumber { get; init; }

    /// <summary>
    /// Gets the sector size in bytes (typically 2048).
    /// </summary>
    public ushort SectorSize { get; init; }

    /// <summary>
    /// Gets the path table length in bytes.
    /// </summary>
    public uint PathTableLength { get; init; }

    /// <summary>
    /// Gets the number of the first sector in the first big endian path table.
    /// </summary>
    public uint FirstSectorFirst { get; init; }

    /// <summary>
    /// Gets the number of the first sector in the second big endian path table, or zero if none exists.
    /// </summary>
    public uint FirstSectorSecond { get; init; }

    /// <summary>
    /// Gets the root directory record for the volume.
    /// </summary>
    public DirectoryRecord DirectoryRecord { get; init; }

    /// <summary>
    /// Gets the volume set identifier.
    /// </summary>
    public string VolumeSetIdentifier { get; init; }

    /// <summary>
    /// Gets the publisher identifier.
    /// </summary>
    public string PublisherIdentifier { get; init; }

    /// <summary>
    /// Gets the data preparer identifier.
    /// </summary>
    public string DataPreparerIdentifier { get; init; }

    /// <summary>
    /// Gets the application identifier.
    /// </summary>
    public string ApplicationIdentifier { get; init; }

    /// <summary>
    /// Gets the copyright file identifier.
    /// </summary>
    public string CopyRightFileIdentifier { get; init; }

    /// <summary>
    /// Gets the abstract file identifier.
    /// </summary>
    public string AbstractFileIdentifier { get; init; }

    /// <summary>
    /// Gets the bibliographical file identifier.
    /// </summary>
    public string BibliographyFileIdentifier { get; init; }

    /// <summary>
    /// Gets the date and time when the volume was created.
    /// </summary>
    public DateTime VolumeCreation { get; init; }

    /// <summary>
    /// Gets the date and time of the most recent volume modification.
    /// </summary>
    public DateTime VolumeModification { get; init; }

    /// <summary>
    /// Gets the date and time when the volume expires.
    /// </summary>
    public DateTime VolumeExpires { get; init; }

    /// <summary>
    /// Gets the date and time when the volume becomes effective.
    /// </summary>
    public DateTime VolumeEffective { get; init; }

    /// <summary>
    /// Creates a VolumeDescription from an ISO 9660 disc stream by reading the primary volume descriptor.
    /// The primary volume descriptor is located at sector 16.
    /// </summary>
    /// <param name="stream">The ISO 9660 disc stream to read from.</param>
    /// <returns>A VolumeDescription instance containing the parsed volume metadata.</returns>
    public static VolumeDescription Create(Stream stream)
    {
        var sector = new byte[2048];
        var bufferLen = 0;

        lock (stream)
        {
            //skip the first 16 sectors
            stream.Seek(16 * sector.Length, SeekOrigin.Begin);
            bufferLen = stream.Read(sector, 0, sector.Length);
        }
        return new VolumeDescription(sector, Encoding.ASCII, stream);
    }

    /// <summary>
    /// Gets or sets the base stream for the ISO 9660 disc.
    /// </summary>
    protected Stream BaseStream { get; set; }

    #region IEnumerable<DirectoryRecord> Members

    /// <summary>
    /// Returns an enumerator that iterates through the root directory records.
    /// </summary>
    /// <returns>An enumerator for the root directory record collection.</returns>
    public IEnumerator<DirectoryRecord> GetEnumerator() => (DirectoryRecord ?? Enumerable.Empty<DirectoryRecord>()).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the root directory records.
    /// </summary>
    /// <returns>An enumerator for the root directory record collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Disposes the volume description and releases the underlying base stream.
    /// </summary>
    public void Dispose() => BaseStream?.Dispose();

    #endregion
}
