using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// Reads dates and place hints written in a path: the most specific date in any folder or file name (deepest first on a tie), and the words left
/// in that component as a place hint. Pure; the caller passes the current year, which only bounds plausible years.
/// </summary>
public static partial class PathDateParser
{
    private static readonly HashSet<string> Generic = new(StringComparer.Ordinal)
    {
        "photos", "pictures", "pics", "images", "image", "dcim", "camera", "backup", "backups", "unsorted", "new folder", "misc",
        "scans", "scan", "media", "download", "downloads", "documents", "files", "my pictures", "my documents", "cameraroll",
        "camera roll", "sorted", "originals", "export", "exports", "thumbs", "temp", "tmp", "data", "stuff", "old", "new",
    };

    [GeneratedRegex(@"(?<!\d)((?:19|20)\d\d)[-_. ]?(0[1-9]|1[0-2])[-_. ]?(0[1-9]|[12]\d|3[01])(?!\d)", RegexOptions.None, 1000)]
    private static partial Regex Ymd();

    [GeneratedRegex(@"(?<!\d)(0[1-9]|1[0-2])[-_. ]?(0[1-9]|[12]\d|3[01])[-_. ]?((?:19|20)\d\d)(?!\d)", RegexOptions.None, 1000)]
    private static partial Regex Mdy();

    [GeneratedRegex(@"(?<!\d)((?:19|20)\d\d)[-_. ]?(0[1-9]|1[0-2])(?!\d)", RegexOptions.None, 1000)]
    private static partial Regex Ym();

    [GeneratedRegex(@"(?<![\d.])((?:19|20)\d\d)(?![\d])", RegexOptions.None, 1000)]
    private static partial Regex Y();

    [GeneratedRegex(@"\.[A-Za-z0-9]{1,5}$", RegexOptions.None, 1000)]
    private static partial Regex Extension();

    [GeneratedRegex(@"[_\-.]+", RegexOptions.None, 1000)]
    private static partial Regex Separators();

    [GeneratedRegex(@"(?<=[a-z])(?=[A-Z])", RegexOptions.None, 1000)]
    private static partial Regex CamelBoundary();

    [GeneratedRegex(@"\b(IMG|DSC|DSCN|DSCF|PXL|MVI|VID|WP|P)\s*\d*\b", RegexOptions.None, 1000)]
    private static partial Regex CameraPrefix();

    [GeneratedRegex(@"\d+", RegexOptions.None, 1000)]
    private static partial Regex Digits();

    [GeneratedRegex("[A-Za-z]{3}", RegexOptions.None, 1000)]
    private static partial Regex ThreeLetters();

    [GeneratedRegex(@"[\\/]", RegexOptions.None, 1000)]
    private static partial Regex Slash();

    /// <summary>Finds the dates written in one path component, most specific form first.</summary>
    /// <param name="text">One folder or file name.</param>
    /// <param name="thisYear">The current year; a year after next year is not plausible.</param>
    /// <returns>The date, its precision, the matched text and the rule (<c>ymd</c>, <c>mdy</c>, <c>ym</c>, <c>y</c>).</returns>
    public static IReadOnlyList<(DateOnly Date, DatePrecision Precision, string Matched, string Rule)> FindDates(string text, int thisYear)
    {
        ArgumentNullException.ThrowIfNull(text);
        var found = new List<(DateOnly, DatePrecision, string, string)>();
        var used = new List<(int Start, int End)>();
        foreach (var (rule, rx, precision) in new[] { ("ymd", Ymd(), DatePrecision.Day), ("mdy", Mdy(), DatePrecision.Day), ("ym", Ym(), DatePrecision.Month), ("y", Y(), DatePrecision.Year) })
        {
            foreach (Match m in rx.Matches(text))
            {
                if (used.Any(u => m.Index < u.End && u.Start < m.Index + m.Length))
                    continue;
                var g = m.Groups.Cast<Group>().Skip(1).Select(x => int.Parse(x.Value, CultureInfo.InvariantCulture)).ToArray();
                var (y, mo, d) = rule switch { "ymd" => (g[0], g[1], g[2]), "mdy" => (g[2], g[0], g[1]), "ym" => (g[0], g[1], 1), _ => (g[0], 1, 1) };
                if (y < 1900 || y > thisYear + 1 || mo < 1 || mo > 12 || d < 1 || d > DateTime.DaysInMonth(y, mo))
                    continue;
                used.Add((m.Index, m.Index + m.Length));
                found.Add((new DateOnly(y, mo, d), precision, m.Value, rule));
            }
        }

        return found;
    }

    /// <summary>Makes a readable place or event hint from the component that carried the date: the date removed, camel case and separators split, camera prefixes and numbers dropped.</summary>
    /// <param name="component">The folder or file name.</param>
    /// <param name="matched">The date text to remove, or null.</param>
    /// <returns>The words, or null when nothing meaningful is left (fewer than four characters, no word of three letters, or a generic folder name).</returns>
    public static string? Words(string component, string? matched)
    {
        ArgumentNullException.ThrowIfNull(component);
        var s = matched is null ? component : component.Replace(matched, " ", StringComparison.Ordinal);
        s = Extension().Replace(s, string.Empty);
        s = Separators().Replace(s, " ");
        s = CamelBoundary().Replace(s, " ");
        s = CameraPrefix().Replace(s, " ");
        s = Digits().Replace(s, " ");
        s = string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (s.Length < 4 || Generic.Contains(s.ToLowerInvariant()) || !ThreeLetters().IsMatch(s))
            return null;
        return s;
    }

    /// <summary>Finds the best date in a path and the place hint beside it; either may be absent.</summary>
    /// <param name="path">A path with <c>/</c> or <c>\</c>; text after <c>!/</c> is a path inside an archive.</param>
    /// <param name="thisYear">The current year.</param>
    /// <returns>The date and the place.</returns>
    public static (PathDate? Date, PathPlace? Place) Parse(string path, int thisYear)
    {
        ArgumentNullException.ThrowIfNull(path);
        var bang = path.IndexOf("!/", StringComparison.Ordinal);
        var parts = Slash().Split(bang < 0 ? path : path[..bang]).Where(p => p.Length > 0).ToList();
        if (bang >= 0)
            parts.AddRange(Slash().Split(path[(bang + 2)..]).Where(p => p.Length > 0));
        (double Conf, int Depth, DateOnly Date, DatePrecision Precision, string Matched, string Rule, string Component, string Where)? best = null;
        for (var depth = 0; depth < parts.Count; depth++)
        {
            var component = parts[depth];
            var isFile = depth == parts.Count - 1;
            foreach (var (date, precision, matched, rule) in FindDates(isFile ? Extension().Replace(component, string.Empty) : component, thisYear))
            {
                var conf = BaseConfidence(precision) + (isFile ? 0.05 : 0);
                if (best is null || conf > best.Value.Conf || (conf == best.Value.Conf && depth > best.Value.Depth))
                    best = (conf, depth, date, precision, matched, rule, component, isFile ? "file name" : $"folder {Repr(component)}");
            }
        }

        if (best is not { } b)
            return (null, null);
        var found = new PathDate(b.Date, b.Precision, b.Matched, Math.Round(b.Conf, 2, MidpointRounding.ToEven), $"{Repr(b.Matched)} in {b.Where} ({b.Rule})");
        var place = Words(b.Component, b.Matched);
        return (found, place is null ? null : new PathPlace(place, 0.30, $"words left in {b.Where} after the date"));
    }

    private static double BaseConfidence(DatePrecision precision) => precision switch
    {
        DatePrecision.Day => 0.75,
        DatePrecision.Month => 0.60,
        _ => 0.45,
    };

    // Python's repr(str), so the detail sentences equal the ones the earlier Python tool wrote.
    private static string Repr(string s)
    {
        var quote = s.Contains('\'', StringComparison.Ordinal) && !s.Contains('"', StringComparison.Ordinal) ? '"' : '\'';
        var sb = new StringBuilder().Append(quote);
        foreach (var ch in s)
        {
            sb.Append(ch switch
            {
                '\\' => "\\\\",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ when ch == quote => "\\" + ch,
                _ when ch < 0x20 || ch == 0x7f => $"\\x{(int)ch:x2}",
                _ => ch.ToString(),
            });
        }

        return sb.Append(quote).ToString();
    }
}
