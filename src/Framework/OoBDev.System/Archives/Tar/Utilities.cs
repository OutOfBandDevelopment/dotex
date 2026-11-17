using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Provides utility methods for working with TAR (Tape Archive) files, including header parsing and GZip compression/decompression.
/// </summary>
public static class Utilities
{
    /// <summary>The size of a TAR header block in bytes.</summary>
    public const int HeaderBlockSize = 512;

    /// <summary>
    /// Parses a TAR header block (ustar layout). The input is untrusted: it must hold a whole block and the numeric fields must be octal and fit an <see cref="int"/>.
    /// </summary>
    /// <param name="input">The header block, at least <see cref="HeaderBlockSize"/> bytes.</param>
    /// <returns>The parsed header. A blank name yields an empty <see cref="TarHeader.FileName"/>, which marks the end-of-archive block.</returns>
    /// <exception cref="ArgumentException">The block is shorter than <see cref="HeaderBlockSize"/>.</exception>
    /// <exception cref="InvalidDataException">A numeric field is not octal or does not fit.</exception>
    public static TarHeader ToHeader(this ReadOnlySpan<byte> input)
    {
        if (input.Length < HeaderBlockSize)
        {
            throw new ArgumentException($"A TAR header block is {HeaderBlockSize} bytes; got {input.Length}.", nameof(input));
        }

        return new TarHeader
        {
            FileName = Text(input, 0, 100),
            FileMode = Text(input, 100, 8),
            OwnerId = Text(input, 108, 8),
            GroupId = Text(input, 116, 8),
            FileSize = Octal(input, 124, 12),
            LastModifiedTime = Octal(input, 136, 12),
            CheckSum = Text(input, 148, 8),
            FileType = (TarFileType)input[156],
            LinkedFile = Text(input, 157, 100),
        };
    }

    /// <inheritdoc cref="ToHeader(ReadOnlySpan{byte})"/>
    public static TarHeader ToHeader(this byte[] input) => ToHeader((ReadOnlySpan<byte>)input);

    /// <summary>
    /// Extracts an ASCII string from a byte array at the specified position and length.
    /// The resulting string is trimmed of null characters and spaces.
    /// </summary>
    /// <param name="input">The byte array containing ASCII-encoded text.</param>
    /// <param name="index">The starting position in the byte array.</param>
    /// <param name="length">The number of bytes to extract.</param>
    /// <returns>The extracted and trimmed string, or null if the input is null/empty or the result is empty.</returns>
    public static string? ToString(this byte[] input, int index, int length)
    {
        if (input == null || input.Length == 0)
        {
            return null;
        }

        var result = Text(input, index, length);
        return result.Length == 0 ? null : result;
    }

    /// <summary>
    /// Decompresses a GZip-compressed byte array and returns the decompressed data.
    /// </summary>
    /// <param name="input">The GZip-compressed byte array to decompress.</param>
    /// <returns>The decompressed byte array, or null if the input is null or empty.</returns>
    public static byte[]? Decompress(this byte[] input)
    {
        if (input == null || input.Length < 1)
        {
            return null;
        }

        using var compressedData = new MemoryStream(input);
        using var decompressedData = new MemoryStream();
        using (var deflateDecompress = new GZipStream(compressedData, CompressionMode.Decompress, true))
        {
            deflateDecompress.CopyTo(decompressedData);
        }

        return decompressedData.ToArray();
    }

    /// <summary>
    /// Creates a GZip decompression stream wrapper around an input stream.
    /// The underlying stream will be closed when the GZipStream is disposed.
    /// </summary>
    /// <param name="input">The GZip-compressed input stream.</param>
    /// <returns>A GZipStream configured for decompression.</returns>
    public static Stream Decompress(this Stream input) =>
        new GZipStream(input, CompressionMode.Decompress, false);

    private static string Text(ReadOnlySpan<byte> block, int index, int length) =>
        Encoding.ASCII.GetString(block.Slice(index, length)).Trim('\0', ' ');

    private static int Octal(ReadOnlySpan<byte> block, int index, int length)
    {
        var text = Text(block, index, length);
        long value = 0;
        foreach (var c in text)
        {
            if (c is < '0' or > '7')
            {
                throw new InvalidDataException($"Tar field at offset {index} is not octal: '{text}'.");
            }

            value = (value << 3) | (uint)(c - '0');
            if (value > int.MaxValue)
            {
                throw new InvalidDataException($"Tar field at offset {index} does not fit 32 bits: '{text}'.");
            }
        }

        return (int)value;
    }
}
