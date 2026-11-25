using System;
using System.Buffers.Binary;

namespace OoBDev.Media.Metadata;

/// <summary>
/// Finds the TIFF block that holds the EXIF data inside an image file.
/// </summary>
public static class ExifLocator
{
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static ReadOnlySpan<byte> ExifHeader => "Exif\0\0"u8;

    /// <summary>
    /// Locates the TIFF block in a TIFF file (the file itself), a JPEG (APP1), a PNG (<c>eXIf</c>) or a WebP (<c>EXIF</c>).
    /// </summary>
    /// <param name="file">The file bytes.</param>
    /// <returns>The TIFF block, or an empty span when the file has none or is not one of these formats.</returns>
    public static ReadOnlySpan<byte> Find(ReadOnlySpan<byte> file)
    {
        if (file.Length > 8 && IsTiffHeader(file))
            return file;
        if (file.Length > 4 && file[0] == 0xFF && file[1] == 0xD8)
            return FindInJpeg(file);
        if (file.Length > 8 && file[..8].SequenceEqual(PngSignature))
            return FindInPng(file);
        if (file.Length > 12 && file[..4].SequenceEqual("RIFF"u8) && file.Slice(8, 4).SequenceEqual("WEBP"u8))
            return FindInWebP(file);
        if (HeicContainer.IsHeic(file))
            return HeicContainer.Read(file)?.Tiff ?? default;
        return default;
    }

    private static bool IsTiffHeader(ReadOnlySpan<byte> f) =>
        (f[0] == 'I' && f[1] == 'I' && f[2] == 42 && f[3] == 0) || (f[0] == 'M' && f[1] == 'M' && f[2] == 0 && f[3] == 42);

    private static ReadOnlySpan<byte> FindInJpeg(ReadOnlySpan<byte> f)
    {
        var i = 2;
        while (i + 4 <= f.Length && f[i] == 0xFF)
        {
            var marker = f[i + 1];
            if (marker is 0xD9 or 0xDA)
                break;
            var length = BinaryPrimitives.ReadUInt16BigEndian(f[(i + 2)..]);
            if (marker == 0xE1 && length >= 8 && Fits(f, i + 4, length - 2) && f.Slice(i + 4, 6).SequenceEqual(ExifHeader))
                return f.Slice(i + 10, length - 8);
            i += 2 + length;
        }

        return default;
    }

    private static ReadOnlySpan<byte> FindInPng(ReadOnlySpan<byte> f)
    {
        var i = 8;
        while (i + 12 <= f.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(f[i..]);
            if (length > int.MaxValue)
                break;
            if (f.Slice(i + 4, 4).SequenceEqual("eXIf"u8))
                return Fits(f, i + 8, (int)length) ? f.Slice(i + 8, (int)length) : default;
            i += 12 + (int)length;
        }

        return default;
    }

    private static ReadOnlySpan<byte> FindInWebP(ReadOnlySpan<byte> f)
    {
        var i = 12;
        while (i + 8 <= f.Length)
        {
            var length = BinaryPrimitives.ReadUInt32LittleEndian(f[(i + 4)..]);
            if (length > int.MaxValue)
                break;
            if (f.Slice(i, 4).SequenceEqual("EXIF"u8))
            {
                if (!Fits(f, i + 8, (int)length))
                    return default;
                var blob = f.Slice(i + 8, (int)length);
                return blob.Length > 6 && blob[..6].SequenceEqual(ExifHeader) ? blob[6..] : blob;
            }

            i += 8 + (int)length + ((int)length & 1);
        }

        return default;
    }

    private static bool Fits(ReadOnlySpan<byte> f, int start, int length) => length >= 0 && start >= 0 && start + (long)length <= f.Length;
}
