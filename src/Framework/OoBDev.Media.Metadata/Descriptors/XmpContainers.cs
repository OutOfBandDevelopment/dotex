using System;
using System.Buffers.Binary;
using System.Text;

namespace OoBDev.Media.Metadata.Descriptors;

/// <summary>Finds the XMP packet in a PNG (an uncompressed <c>iTXt</c> chunk keyed <c>XML:com.adobe.xmp</c>) or a WebP (the <c>XMP </c> chunk of the RIFF).</summary>
internal static class XmpContainers
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private const string PngKeyword = "XML:com.adobe.xmp";

    public static string? Find(byte[] file)
    {
        if (file.AsSpan().StartsWith(PngSignature))
            return FromPng(file);
        if (file.Length >= 12 && file.AsSpan(0, 4).SequenceEqual("RIFF"u8) && file.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return FromWebP(file);
        if (file.Length >= 8 && ((file[0] == 'I' && file[1] == 'I' && file[2] == 42 && file[3] == 0) || (file[0] == 'M' && file[1] == 'M' && file[2] == 0 && file[3] == 42)))
            return FromTiff(file);
        return null;
    }

    private static string? FromTiff(byte[] file)
    {
        var tiff = TiffBlock.Open(file);
        if (tiff is null)
            return null;
        var xmp = TiffBlock.Find(tiff.Ifd(tiff.FirstIfd), 700);
        return xmp is { Type: 1 or 7 } e ? tiff.Utf8(e.At, e.Count) : null;
    }

    private static string? FromPng(byte[] file)
    {
        var i = PngSignature.Length;
        while (i + 12 <= file.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(file.AsSpan(i));
            if (length > file.Length - i - 12)
                return null;
            var type = file.AsSpan(i + 4, 4);
            var data = file.AsSpan(i + 8, (int)length);
            if (type.SequenceEqual("iTXt"u8) && Itxt(data) is { } xml)
                return xml;
            if (type.SequenceEqual("IEND"u8))
                return null;
            i += 12 + (int)length;
        }

        return null;
    }

    private static string? Itxt(ReadOnlySpan<byte> data)
    {
        var keyEnd = data.IndexOf((byte)0);
        if (keyEnd < 0 || !data[..keyEnd].SequenceEqual(Encoding.ASCII.GetBytes(PngKeyword)) || data.Length < keyEnd + 3)
            return null;
        if (data[keyEnd + 1] != 0)
            return null; // compressed text is not read
        var rest = data[(keyEnd + 3)..];
        var langEnd = rest.IndexOf((byte)0);
        if (langEnd < 0)
            return null;
        rest = rest[(langEnd + 1)..];
        var translatedEnd = rest.IndexOf((byte)0);
        if (translatedEnd < 0)
            return null;
        return Encoding.UTF8.GetString(rest[(translatedEnd + 1)..]);
    }

    private static string? FromWebP(byte[] file)
    {
        var i = 12;
        while (i + 8 <= file.Length)
        {
            var size = BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(i + 4));
            if (size > file.Length - i - 8)
                return null;
            if (file.AsSpan(i, 4).SequenceEqual("XMP "u8))
                return Encoding.UTF8.GetString(file, i + 8, (int)size);
            i += 8 + (int)size + (int)(size & 1);
        }

        return null;
    }
}
