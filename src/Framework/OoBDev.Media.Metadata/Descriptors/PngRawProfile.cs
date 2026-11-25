using System;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OoBDev.Media.Metadata.Descriptors;

/// <summary>
/// Reads an ImageMagick or ExifTool style "raw profile" from a PNG: a <c>zTXt</c> (deflated) or <c>tEXt</c> chunk keyed
/// <c>Raw profile type &lt;name&gt;</c> whose text is a name line, a length line and the profile bytes as hex.
/// </summary>
internal static class PngRawProfile
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private const int MaxProfile = 1 << 24;

    /// <summary>The decoded profile bytes for <paramref name="name"/> (for example <c>iptc</c>), or <see langword="null"/>.</summary>
    public static byte[]? Find(byte[] file, string name)
    {
        if (!file.AsSpan().StartsWith(PngSignature))
            return null;
        var keyword = Encoding.ASCII.GetBytes("Raw profile type " + name);
        var i = PngSignature.Length;
        while (i + 12 <= file.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(file.AsSpan(i));
            if (length > file.Length - i - 12)
                return null;
            var type = file.AsSpan(i + 4, 4);
            var data = file.AsSpan(i + 8, (int)length);
            var isZ = type.SequenceEqual("zTXt"u8);
            if ((isZ || type.SequenceEqual("tEXt"u8)) && Text(data, keyword, isZ) is { } text && Decode(text) is { } profile)
                return profile;
            if (type.SequenceEqual("IEND"u8))
                return null;
            i += 12 + (int)length;
        }

        return null;
    }

    private static string? Text(ReadOnlySpan<byte> data, byte[] keyword, bool deflated)
    {
        var keyEnd = data.IndexOf((byte)0);
        if (keyEnd < 0 || !data[..keyEnd].SequenceEqual(keyword))
            return null;
        if (!deflated)
            return Encoding.Latin1.GetString(data[(keyEnd + 1)..]);
        if (data.Length < keyEnd + 2 || data[keyEnd + 1] != 0)
            return null;
        try
        {
            using var zlib = new ZLibStream(new MemoryStream(data[(keyEnd + 2)..].ToArray()), CompressionMode.Decompress);
            using var output = new MemoryStream();
            var buffer = new byte[4096];
            int read;
            while ((read = zlib.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
                if (output.Length > MaxProfile * 2L)
                    return null;
            }

            return Encoding.Latin1.GetString(output.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private static byte[]? Decode(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.None);
        var at = 0;
        while (at < lines.Length && lines[at].Trim().Length == 0)
            at++;
        if (at + 1 >= lines.Length || !int.TryParse(lines[at + 1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var size) || size is <= 0 or > MaxProfile)
            return null;
        var bytes = new byte[size];
        var n = 0;
        var high = -1;
        for (var line = at + 2; line < lines.Length && n < size; line++)
        {
            foreach (var c in lines[line])
            {
                var v = Uri.IsHexDigit(c) ? Convert.ToInt32(c.ToString(), 16) : -1;
                if (v < 0)
                    continue;
                if (high < 0)
                    high = v;
                else
                {
                    bytes[n++] = (byte)((high << 4) | v);
                    high = -1;
                    if (n == size)
                        break;
                }
            }
        }

        return n == size ? bytes : null;
    }
}
