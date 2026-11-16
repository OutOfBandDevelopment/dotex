using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OoBDev.Documents.Markdown;

/// <summary>Where a document's Markdown goes under an extract folder: one folder per root, slugged sub-folders and a file name with the first eight characters of the content hash.</summary>
public static partial class ExtractPaths
{
    /// <summary>The folder name for a file that sits directly in the catalog root.</summary>
    public const string Loose = "(root)";

    /// <summary>The Markdown path for a stored catalog path (<c>root/folder/name.ext</c>, optionally prefixed <c>@mount/</c>).</summary>
    /// <param name="extractDir">The extract folder.</param>
    /// <param name="storedPath">The stored path with forward slashes.</param>
    /// <param name="sha256">The content hash, at least eight characters.</param>
    public static string Markdown(string extractDir, string storedPath, string sha256)
    {
        ArgumentNullException.ThrowIfNull(storedPath);
        ArgumentNullException.ThrowIfNull(sha256);
        var (prefix, rest) = storedPath.StartsWith('@') && storedPath.IndexOf('/', StringComparison.Ordinal) is var slash and > 0
            ? (storedPath[..(slash + 1)], storedPath[(slash + 1)..])
            : ("", storedPath);
        var parts = rest.Split('/');
        var top = parts.Length == 1 ? prefix + Loose : prefix + parts[0];
        var rel = parts.Length == 1 ? parts : parts[1..];
        var dir = Path.Combine(extractDir, top.ToLowerInvariant().Replace('/', Path.DirectorySeparatorChar));
        foreach (var folder in rel[..^1])
        {
            dir = Path.Combine(dir, Slug(folder));
        }

        return Path.Combine(dir, $"{Slug(Path.GetFileNameWithoutExtension(rel[^1]))}-{sha256[..8]}.md");
    }

    /// <summary>Lower-case letters and digits with single dashes, at most 80 characters, never empty.</summary>
    /// <param name="value">The text to slug.</param>
    public static string Slug(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var t = NonAlnum().Replace(value.ToLowerInvariant(), "-").Trim('-');
        if (t.Length > 80)
        {
            t = t[..80];
        }

        return t.Length == 0 ? "x" : t;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.None, 1000)]
    private static partial Regex NonAlnum();
}
