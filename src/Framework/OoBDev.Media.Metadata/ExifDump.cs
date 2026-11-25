using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.Media.Metadata;

/// <summary>
/// Every EXIF tag of an image as <c>{name: text}</c>: the main directory and the Exif directory unprefixed, then <c>GPS.</c> and <c>Interop.</c> tags.
/// Byte values over 64 become <c>&lt;N bytes&gt;</c>, sequences over 24 <c>&lt;N values&gt;</c>, text is cut at 300 characters and empty values are dropped.
/// Numbers print the way Python's <c>str()</c> does, so the text equals what a Pillow-based reader produced.
/// </summary>
public static class ExifDump
{
    private const int ExifPointer = 0x8769, GpsPointer = 0x8825, InteropPointer = 0xA005;
    private const int MaxText = 300, MaxBytes = 64, MaxValues = 24;

    /// <summary>Reads every tag from an image file's bytes.</summary>
    /// <param name="file">A JPEG, PNG, WebP or TIFF file.</param>
    /// <returns>The tags by name (the tag number when the name is unknown); empty when there is no readable EXIF.</returns>
    public static IReadOnlyDictionary<string, string> Read(ReadOnlySpan<byte> file)
    {
        try
        {
            var block = TiffBlock.Open(ExifLocator.Find(file));
            return block is null ? new Dictionary<string, string>(StringComparer.Ordinal) : Dump(block);
        }
        catch (Exception e) when (e is ArgumentException or IndexOutOfRangeException or OverflowException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static Dictionary<string, string> Dump(TiffBlock t)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var ifd0 = t.Ifd(t.FirstIfd);
        Put(result, "", ifd0.Where(e => e.Tag is not (ExifPointer or GpsPointer or InteropPointer)), ExifTagNames.Tags, t);

        var exif = TiffBlock.Find(ifd0, ExifPointer) is { } ep ? t.Ifd((int)t.U32(ep.At)) : [];
        Put(result, "", exif, ExifTagNames.Tags, t);
        if (TiffBlock.Find(ifd0, GpsPointer) is { } gp)
            Put(result, "GPS.", t.Ifd((int)t.U32(gp.At)), ExifTagNames.Gps, t);
        if (TiffBlock.Find(exif, InteropPointer) is { } ip)
            Put(result, "Interop.", t.Ifd((int)t.U32(ip.At)), ExifTagNames.Tags, t);
        return result;
    }

    private static void Put(Dictionary<string, string> into, string prefix, IEnumerable<TiffBlock.Entry> entries, IReadOnlyDictionary<int, string> names, TiffBlock t)
    {
        foreach (var e in entries)
        {
            var s = Text(t, e).Replace("\0", "", StringComparison.Ordinal).Trim();
            if (s.Length == 0)
                continue;
            into[prefix + (names.TryGetValue(e.Tag, out var n) ? n : e.Tag.ToString(CultureInfo.InvariantCulture))] = s.Length > MaxText ? s[..MaxText] : s;
        }
    }

    private static string Text(TiffBlock t, TiffBlock.Entry e) => e.Type switch
    {
        2 => t.Utf8(e.At, e.Count).TrimEnd('\0'),
        1 or 7 => e.Count > MaxBytes ? $"<{e.Count} bytes>" : t.Utf8(e.At, e.Count),
        6 => Sequence(e, i => ((sbyte)t.Byte(e.At + i)).ToString(CultureInfo.InvariantCulture)),
        _ => Sequence(e, i => Item(t, e, i)),
    };

    private static string Sequence(TiffBlock.Entry e, Func<int, string> item)
    {
        if (e.Count > MaxValues)
            return $"<{e.Count} values>";
        var parts = Enumerable.Range(0, e.Count).Select(item).ToList();
        return e.Count == 1 ? parts[0] : "(" + string.Join(", ", parts) + ")";
    }

    private static string Item(TiffBlock t, TiffBlock.Entry e, int i)
    {
        var at = e.At;
        switch (e.Type)
        {
            case 3: return t.U16(at + (i * 2)).ToString(CultureInfo.InvariantCulture);
            case 4: return t.U32(at + (i * 4)).ToString(CultureInfo.InvariantCulture);
            case 8: return ((short)t.U16(at + (i * 2))).ToString(CultureInfo.InvariantCulture);
            case 9: return ((int)t.U32(at + (i * 4))).ToString(CultureInfo.InvariantCulture);
            case 11: return PythonText.Float(BitConverter.Int32BitsToSingle((int)t.U32(at + (i * 4))));
            case 12:
                return PythonText.Float(BitConverter.Int64BitsToDouble(t.Int64(at + (i * 8))));
            case 5: return Rational(t.U32(at + (i * 8)), t.U32(at + (i * 8) + 4));
            case 10: return Rational((int)t.U32(at + (i * 8)), (int)t.U32(at + (i * 8) + 4));
            default: return "";
        }
    }

    private static string Rational(double numerator, double denominator) => denominator == 0 ? "nan" : PythonText.Float(numerator / denominator);
}
