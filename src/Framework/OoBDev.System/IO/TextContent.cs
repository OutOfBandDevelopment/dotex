using System;
using System.IO;

namespace OoBDev.System.IO;

/// <summary>
/// Tells text from binary content by looking at the bytes instead of the file extension.
/// </summary>
public static class TextContent
{
    /// <summary>
    /// The number of leading bytes inspected.
    /// </summary>
    public const int SampleSize = 8192;

    /// <summary>
    /// Whether the bytes look like text: no NUL bytes (a UTF-16 or UTF-32 byte order mark is accepted) and, for the rest, no more than a few percent control characters.
    /// </summary>
    /// <param name="sample">The leading bytes of the content.</param>
    /// <returns><see langword="true"/> for text or empty content.</returns>
    public static bool LooksLikeText(ReadOnlySpan<byte> sample)
    {
        if (sample.IsEmpty) return true;

        var wide = sample.StartsWith(new byte[] { 0xFF, 0xFE }) || sample.StartsWith(new byte[] { 0xFE, 0xFF });
        if (!wide && sample.Contains((byte)0)) return false;

        var control = 0;
        foreach (var b in sample)
        {
            if (b == 0) continue;
            if (b < 0x20 && b is not (byte)'\t' and not (byte)'\n' and not (byte)'\r' and not 0x0C and not 0x1B) control++;
        }
        return control * 100 <= sample.Length * 5;
    }

    /// <summary>
    /// Whether the file looks like text.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns><see langword="true"/> when the leading bytes look like text.</returns>
    public static bool FileLooksLikeText(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var buffer = new byte[SampleSize];
        var read = stream.Read(buffer, 0, buffer.Length);
        return LooksLikeText(buffer.AsSpan(0, read));
    }
}
