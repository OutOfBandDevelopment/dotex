using System;
using System.Collections.Generic;
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
    public static ExifFields Read(ReadOnlySpan<byte> file) => ReadWithProblems(file).Fields;

    /// <summary>Reads the fields and reports what was damaged: entries that were cut off or had no valid type or size, tags of the wrong type, a capture time that does not parse, and GPS values out of range.</summary>
    /// <param name="file">A JPEG, PNG, WebP, HEIC or TIFF file.</param>
    /// <returns>The valid fields and the problems; a file without an EXIF block gives empty fields and no problems.</returns>
    public static ExifReadResult ReadWithProblems(ReadOnlySpan<byte> file)
    {
        var problems = new List<string>();
        try
        {
            var located = ExifLocator.Find(file);
            if (located.IsEmpty)
                return new ExifReadResult(ExifFields.Empty, problems);
            var block = TiffBlock.Open(located);
            if (block is null)
            {
                problems.Add("The EXIF block has no valid TIFF header.");
                return new ExifReadResult(ExifFields.Empty, problems);
            }

            return new ExifReadResult(Parse(block, problems), problems);
        }
        catch (Exception e) when (e is ArgumentException or IndexOutOfRangeException or OverflowException or FormatException)
        {
            problems.Add($"The EXIF block could not be read: {e.GetType().Name}.");
            return new ExifReadResult(ExifFields.Empty, problems);
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

    private static ExifFields Parse(TiffBlock t, List<string> problems)
    {
        var ifd0 = t.Ifd(t.FirstIfd, out var skipped0);
        if (skipped0 > 0)
            problems.Add($"{skipped0} entries of the first directory were cut off or had an invalid type or size.");

        string? Ascii(TiffBlock.Entry? entry) => entry is { Type: 2 } e ? t.Utf8(e.At, e.Count).TrimEnd('\0') : null;

        string? Text(int tag, string name)
        {
            var entry = TiffBlock.Find(ifd0, tag);
            if (entry is { Type: not 2 })
                problems.Add($"Tag {tag} ({name}) has type {entry.Value.Type}, expected text.");
            return Ascii(entry);
        }

        var make = Text(MakeTag, "Make");
        var model = Text(ModelTag, "Model");
        int? orientation = null;
        if (TiffBlock.Find(ifd0, OrientationTag) is { } o)
        {
            if (o is { Type: 3, Count: >= 1 })
                orientation = t.U16(o.At);
            else
                problems.Add($"Tag {OrientationTag} (Orientation) has type {o.Type} and count {o.Count}, expected one short.");
        }

        string? date = null;
        if (TiffBlock.Find(ifd0, ExifPointer) is { } exifPointer)
        {
            var exifIfd = t.Ifd((int)t.U32(exifPointer.At), out var skippedExif);
            if (skippedExif > 0)
                problems.Add($"{skippedExif} entries of the Exif directory were cut off or had an invalid type or size.");
            date = Ascii(TiffBlock.Find(exifIfd, DateTimeOriginalTag));
        }

        if (string.IsNullOrEmpty(date))
            date = Ascii(TiffBlock.Find(ifd0, DateTimeTag));
        var taken = ParseTaken(date);
        if (!string.IsNullOrWhiteSpace(date) && taken is null)
            problems.Add($"The capture time '{Clean(date)}' does not parse.");

        var (lat, lon) = Gps(t, ifd0, Ascii, problems);
        return new ExifFields(Clean(make), Clean(model), orientation, taken, lat, lon);
    }

    private static (double? Lat, double? Lon) Gps(TiffBlock t, List<TiffBlock.Entry> ifd0, Func<TiffBlock.Entry?, string?> ascii, List<string> problems)
    {
        if (TiffBlock.Find(ifd0, GpsPointer) is not { } pointer)
            return (null, null);
        var gps = t.Ifd((int)t.U32(pointer.At), out var skipped);
        if (skipped > 0)
            problems.Add($"{skipped} entries of the GPS directory were cut off or had an invalid type or size.");
        if (TiffBlock.Find(gps, 2) is not { Type: 5, Count: >= 3 } la || TiffBlock.Find(gps, 4) is not { Type: 5, Count: >= 3 } lo)
        {
            if (TiffBlock.Find(gps, 2) is not null || TiffBlock.Find(gps, 4) is not null)
                problems.Add("The GPS latitude or longitude is missing or is not three rationals.");
            return (null, null);
        }

        double Degrees(TiffBlock.Entry value, string? reference) =>
            (t.Rational(value, 0) + (t.Rational(value, 1) / 60) + (t.Rational(value, 2) / 3600)) * (reference is "S" or "W" ? -1 : 1);

        var a = Degrees(la, ascii(TiffBlock.Find(gps, 1)));
        var b = Degrees(lo, ascii(TiffBlock.Find(gps, 3)));
        if (a is >= -90 and <= 90 && b is >= -180 and <= 180 && !(a == 0 && b == 0))
            return (a, b);
        problems.Add("The GPS position is out of range or zero and was dropped.");
        return (null, null);
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;
        var s = value.Replace("\0", "", StringComparison.Ordinal).Trim();
        return s.Length > 100 ? s[..100] : s;
    }
}
