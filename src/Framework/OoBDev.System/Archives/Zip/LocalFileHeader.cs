using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Archives.Zip;

/// <summary>
/// Represents the local file header structure in a ZIP archive.
/// </summary>
public struct LocalFileHeader
{
    /// <summary>
    /// Local file header signature (0x04034b50).
    /// </summary>
    public int Signature;

    /// <summary>
    /// Version needed to extract the file.
    /// </summary>
    public short Version;

    /// <summary>
    /// General purpose bit flags.
    /// </summary>
    public short BitFlags;

    /// <summary>
    /// Compression method used for this file.
    /// </summary>
    public CompressionMethodType CompressionMethod;

    /// <summary>
    /// Last modification time of the file (MS-DOS format).
    /// </summary>
    public short LastModifyTime;

    /// <summary>
    /// Last modification date of the file (MS-DOS format).
    /// </summary>
    public short LastModifyDate;

    /// <summary>
    /// CRC-32 checksum of the uncompressed file data.
    /// </summary>
    public int CRC32;

    /// <summary>
    /// Size of the compressed file data in bytes.
    /// </summary>
    public int CompressedSize;

    /// <summary>
    /// Size of the uncompressed file data in bytes.
    /// </summary>
    public int UncompressedSize;

    /// <summary>
    /// Length of the file name field in bytes.
    /// </summary>
    public short FileNameLength;

    /// <summary>
    /// Length of the extra field in bytes.
    /// </summary>
    public short ExtraFieldLength;

    /// <summary>
    /// The file name.
    /// </summary>
    public string FileName;

    /// <summary>
    /// The extra field data.
    /// </summary>
    public string ExtraField;

    /// <summary>
    /// Gets the total size of the header in bytes, including variable-length fields.
    /// </summary>
    public readonly int HeaderSize => 4 + 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4 + 2 + 2 + FileNameLength + ExtraFieldLength;

    /// <summary>
    /// Converts the local file header to its binary representation.
    /// </summary>
    /// <param name="localFileHeader">The header to convert.</param>
    public static implicit operator byte[](LocalFileHeader localFileHeader)
    {
        List<byte> data =
        [
            .. BitConverter.GetBytes(localFileHeader.Signature),
            .. BitConverter.GetBytes(localFileHeader.Version),
            .. BitConverter.GetBytes(localFileHeader.BitFlags),
            .. BitConverter.GetBytes((short)localFileHeader.CompressionMethod),
            .. BitConverter.GetBytes(localFileHeader.LastModifyTime),
            .. BitConverter.GetBytes(localFileHeader.LastModifyDate),
            .. BitConverter.GetBytes(localFileHeader.CRC32),
            .. BitConverter.GetBytes(localFileHeader.CompressedSize),
            .. BitConverter.GetBytes(localFileHeader.UncompressedSize),
        ];

        var fileName = Encoding.ASCII.GetBytes(localFileHeader.FileName);
        var extraField = Encoding.ASCII.GetBytes(localFileHeader.ExtraField);

        data.AddRange(BitConverter.GetBytes(fileName.Length));
        data.AddRange(BitConverter.GetBytes(extraField.Length));

        data.AddRange(fileName);
        data.AddRange(extraField);

        return [.. data];
    }

    /// <summary>
    /// Parses a binary representation into a local file header structure.
    /// </summary>
    /// <param name="rawFileheader">The binary data to parse.</param>
    public static implicit operator LocalFileHeader(byte[] rawFileheader)
    {
        LocalFileHeader localFileHeader = new()
        {
            Signature = BitConverter.ToInt32(rawFileheader, 0),
            Version = BitConverter.ToInt16(rawFileheader, 4),
            BitFlags = BitConverter.ToInt16(rawFileheader, 4 + 2),
            CompressionMethod = (CompressionMethodType)BitConverter.ToInt16(rawFileheader, 4 + 2 + 2),
            LastModifyTime = BitConverter.ToInt16(rawFileheader, 4 + 2 + 2 + 2),
            LastModifyDate = BitConverter.ToInt16(rawFileheader, 4 + 2 + 2 + 2 + 2),
            CRC32 = BitConverter.ToInt32(rawFileheader, 4 + 2 + 2 + 2 + 2 + 2),
            CompressedSize = BitConverter.ToInt32(rawFileheader, 4 + 2 + 2 + 2 + 2 + 2 + 4),
            UncompressedSize = BitConverter.ToInt32(rawFileheader, 4 + 2 + 2 + 2 + 2 + 2 + 4 + 4),
            FileNameLength = BitConverter.ToInt16(rawFileheader, 4 + 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4),
            ExtraFieldLength = BitConverter.ToInt16(rawFileheader, 4 + 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4 + 2),
        };
        var lastPosition = 4 + 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4 + 2 + 2;
        localFileHeader.FileName = Encoding.ASCII.GetString(rawFileheader, lastPosition, localFileHeader.FileNameLength);
        lastPosition += localFileHeader.FileNameLength;
        localFileHeader.ExtraField = Encoding.ASCII.GetString(rawFileheader, lastPosition, localFileHeader.ExtraFieldLength);
        lastPosition += localFileHeader.ExtraFieldLength;
        return localFileHeader;
    }
}
