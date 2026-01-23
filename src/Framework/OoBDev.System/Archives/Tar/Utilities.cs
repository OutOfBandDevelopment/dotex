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
    /// <summary>
    /// Converts a byte array containing TAR header data into a TarHeader object.
    /// Parses the 512-byte TAR header block according to the TAR format specification.
    /// </summary>
    /// <param name="input">The byte array containing the TAR header data (typically 512 bytes).</param>
    /// <returns>A TarHeader object populated with the parsed header information.</returns>
    /// <exception cref="NotSupportedException">Thrown when required header fields are missing or invalid.</exception>
    public static TarHeader ToHeader(this byte[] input) =>
        new()
        {
            FileName = input.ToString(0, 100) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.FileName)}"),
            FileMode = input.ToString(100, 8) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.FileMode)}"),
            OwnerId = input.ToString(108, 8) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.OwnerId)}"),
            GroupId = input.ToString(116, 8) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.GroupId)}"),

            FileSize = Convert.ToInt32(input.ToString(124, 12) ?? "0", 8),
            LastModifiedTime = Convert.ToInt32(input.ToString(136, 12) ?? "0", 8),
            CheckSum = input.ToString(148, 8) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.CheckSum)}"),
            FileType = (TarFileType)input[156],
            LinkedFile = input.ToString(157, 100) ?? throw new NotSupportedException($"Missing {nameof(TarHeader.LinkedFile)}"),
        };

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
            return null;
        else
        {
            var result = Encoding.ASCII.GetString(input, index, length)
                                          .Trim('\0', ' ');
            return result == string.Empty ? null : result;
        }
    }

    /// <summary>
    /// Decompresses a GZip-compressed byte array and returns the decompressed data.
    /// </summary>
    /// <param name="input">The GZip-compressed byte array to decompress.</param>
    /// <returns>The decompressed byte array, or null if the input is null or empty.</returns>
    public static byte[]? Decompress(this byte[] input)
    {
        if (input == null || input.Length < 1)
            return null;

        using var compressedData = new MemoryStream(input);
        using var decompressedData = new MemoryStream();
        using (var deflateDecompress = new GZipStream(compressedData,
                                                      CompressionMode.Decompress,
                                                      true))
        {
            var buffer = new byte[1024];
            int bufferLen;
            do
            {
                bufferLen = deflateDecompress.Read(buffer,
                                                   0,
                                                   buffer.Length);
                if (bufferLen > 0)
                    decompressedData.Write(buffer, 0, bufferLen);
            } while (bufferLen > 0);
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
}
