using System;
using System.Globalization;

namespace OoBDev.Media.Metadata;

/// <summary>
/// Read-only reader for the common EXIF fields (make, model, orientation, capture time, GPS). Unreadable data gives
/// <see cref="ExifFields.Empty"/> or null fields, never an exception, because stripped files and screenshots are normal input.
/// The library logs nothing: the files are private photos and the caller decides what to record.
/// </summary>
public static class ExifReader
{
    private const int MakeTag = 271, ModelTag = 272, OrientationTag = 274, DateTimeTag = 306;
    private const int ExifPointer = 0x8769, GpsPointer = 0x8825, DateTimeOriginalTag = 36867;

    /// <summary>Reads the fields from an image file's bytes.</summary>
    /// <param name="file">A JPEG, PNG, WebP or TIFF file.</param>
    /// <returns>The fields; see <see cref="ExifFields"/>.</returns>
    public static ExifFields Read(ReadOnlySpan<byte> file)
    {
        try
        {
            var block = TiffBlock.Open(ExifLocator.Find(file));
            return block is null ? ExifFields.Empty : Parse(block);
        }
        catch (Exception e) when (e is ArgumentException or IndexOutOfRangeException or OverflowException or FormatException)
        {
            return ExifFields.Empty;
        }
    }

    /// <summary>Parses the capture time text <c>yyyy:MM:dd HH:mm:ss</c> (extra trailing characters are ignored).</summary>
    /// <param name="raw">The EXIF text.</param>
    /// <returns>The time, or <see langword="null"/> when it does not parse.</returns>
    public static DateTime? ParseTaken(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var s = raw.Trim();
        if (s.Length > 19)
            s = s[..19];
        return DateTime.TryParseExact(s, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }

    private static ExifFields Parse(TiffBlock t)
    {
        var ifd0 = t.Ifd(t.FirstIfd);

        string? Ascii(TiffBlock.Entry? entry) => entry is { Type: 2 } e ? t.Utf8(e.At, e.Count).TrimEnd('\0') : null;

        var make = Ascii(TiffBlock.Find(ifd0, MakeTag));
        var model = Ascii(TiffBlock.Find(ifd0, ModelTag));
        int? orientation = TiffBlock.Find(ifd0, OrientationTag) is { Type: 3, Count: >= 1 } o ? t.U16(o.At) : null;

        string? date = null;
        if (TiffBlock.Find(ifd0, ExifPointer) is { } exifPointer)
            date = Ascii(TiffBlock.Find(t.Ifd((int)t.U32(exifPointer.At)), DateTimeOriginalTag));
        if (string.IsNullOrEmpty(date))
            date = Ascii(TiffBlock.Find(ifd0, DateTimeTag));

        var (lat, lon) = Gps(t, ifd0, Ascii);
        return new ExifFields(Clean(make), Clean(model), orientation, ParseTaken(date), lat, lon);
    }

    private static (double? Lat, double? Lon) Gps(TiffBlock t, System.Collections.Generic.List<TiffBlock.Entry> ifd0, Func<TiffBlock.Entry?, string?> ascii)
    {
        if (TiffBlock.Find(ifd0, GpsPointer) is not { } pointer)
            return (null, null);
        var gps = t.Ifd((int)t.U32(pointer.At));
        if (TiffBlock.Find(gps, 2) is not { Type: 5, Count: >= 3 } la || TiffBlock.Find(gps, 4) is not { Type: 5, Count: >= 3 } lo)
            return (null, null);

        double Degrees(TiffBlock.Entry value, string? reference) =>
            (t.Rational(value, 0) + (t.Rational(value, 1) / 60) + (t.Rational(value, 2) / 3600)) * (reference is "S" or "W" ? -1 : 1);

        var a = Degrees(la, ascii(TiffBlock.Find(gps, 1)));
        var b = Degrees(lo, ascii(TiffBlock.Find(gps, 3)));
        return a is >= -90 and <= 90 && b is >= -180 and <= 180 && !(a == 0 && b == 0) ? (a, b) : (null, null);
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;
        var s = value.Replace("\0", "", StringComparison.Ordinal).Trim();
        return s.Length > 100 ? s[..100] : s;
    }
}
