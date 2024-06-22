using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OoBDev.System.Archives.Tar;

public static class Utilities
{
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

    public static string? ToString(this byte[] input, int index, int length)
    {
        if (input == null || input.Length == 0)
            return null;
        else
        {
            string result = Encoding.ASCII.GetString(input, index, length)
                                          .Trim('\0', ' ');
            if (result == string.Empty)
                return null;
            else
                return result;
        }
    }

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
            byte[] buffer = new byte[1024];
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

    public static Stream Decompress(this Stream input) =>
        new GZipStream(input, CompressionMode.Decompress, false);
}
