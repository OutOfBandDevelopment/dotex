using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Extensions.Drawing;

/// <summary>Splits a JPEG stream into its marker segments, for inspecting or repairing metadata such as Exif.</summary>
public static class JpegSegmentReader
{
    private const byte Prefix = 0xFF;
    private const byte StartOfImage = 0xD8;
    private const byte EndOfImage = 0xD9;
    private const byte StartOfScan = 0xDA;

    /// <summary>Reads the segments of a JPEG file.</summary>
    /// <param name="path">The file to read.</param>
    public static IEnumerable<JpegSegment> GetSegments(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        foreach (var segment in GetSegments(stream))
            yield return segment;
    }

    /// <summary>Reads the segments of a JPEG stream up to and including the end-of-image marker.</summary>
    /// <param name="stream">A readable stream positioned at the start-of-image marker.</param>
    /// <exception cref="InvalidDataException">The stream is not a well-formed marker sequence.</exception>
    public static IEnumerable<JpegSegment> GetSegments(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var index = 0;
        var marker = ReadMarker(stream);
        while (marker is { } type)
        {
            if (type == EndOfImage || type == StartOfImage || IsStandalone(type))
            {
                yield return new JpegSegment(index++, type, []);
                if (type == EndOfImage) yield break;
                marker = ReadMarker(stream);
                continue;
            }

            var length = ReadLength(stream);
            var data = new byte[length - 2];
            stream.ReadExactly(data);

            if (type == StartOfScan)
            {
                using var scan = new MemoryStream();
                scan.Write(data);
                marker = ReadScanData(stream, scan);
                yield return new JpegSegment(index++, type, scan.ToArray());
                continue;
            }

            yield return new JpegSegment(index++, type, data);
            marker = ReadMarker(stream);
        }
    }

    private static bool IsStandalone(byte type) => type is 0x01 or (>= 0xD0 and <= 0xD7);

    private static byte? ReadMarker(Stream stream)
    {
        var prefix = stream.ReadByte();
        if (prefix < 0) return null;
        if (prefix != Prefix) throw new InvalidDataException("Invalid marker prefix.");

        int type;
        do { type = stream.ReadByte(); } while (type == Prefix);
        return type < 0 ? throw new InvalidDataException("The stream ended inside a marker.") : (byte)type;
    }

    private static int ReadLength(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        var length = (buffer[0] << 8) | buffer[1];
        return length < 2 ? throw new InvalidDataException("Invalid segment length.") : length;
    }

    // Entropy-coded data holds 0xFF only as the stuffed pair FF00 or as a restart marker; anything else ends the scan.
    private static byte? ReadScanData(Stream stream, MemoryStream scan)
    {
        while (true)
        {
            var value = stream.ReadByte();
            if (value < 0) return null;
            if (value != Prefix) { scan.WriteByte((byte)value); continue; }

            int next;
            do { next = stream.ReadByte(); } while (next == Prefix);
            if (next < 0) return null;

            if (next == 0x00 || (next >= 0xD0 && next <= 0xD7))
            {
                scan.WriteByte(Prefix);
                scan.WriteByte((byte)next);
                continue;
            }

            return (byte)next;
        }
    }
}
