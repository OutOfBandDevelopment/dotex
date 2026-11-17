using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.System.IO;

/// <summary>
/// Extensions for path strings
/// </summary>
public static class PathEx
{
    private static readonly char[] _wildCards = ['*', '?'];

    /// <summary>
    /// Create parent directory is does not exist
    /// </summary>
    /// <param name="path"></param>
    /// <returns>return input path to support chaining</returns>
    public static string? CreateParentIfNotExists(this string? path)
    {
        var realDir = Path.GetDirectoryName(path);
        if (realDir != null && !Directory.Exists(realDir))
            Directory.CreateDirectory(realDir);
        return path;
    }

    /// <summary>
    /// Determines whether a path string ends with a directory separator character.
    /// </summary>
    /// <param name="path">The path string to check.</param>
    /// <returns>True if the path ends with a directory separator (\ or /); otherwise, false.</returns>
    public static bool EndsInDirectorySeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) ||
        path.EndsWith(Path.AltDirectorySeparatorChar);

    /// <summary>
    /// Normalizes a path by replacing all forward slashes and backslashes with the platform's directory separator.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path, or null if the input is null or whitespace.</returns>
    public static string? FixUpPath(string path) =>
       string.IsNullOrWhiteSpace(path) ? null : string.Join(Path.DirectorySeparatorChar, path.Split('/', '\\'));

    /// <summary>
    /// Extracts the base path from a wildcard path by removing wildcard segments.
    /// For example, "C:\folder\*.txt" returns "C:\folder".
    /// </summary>
    /// <param name="path">The path that may contain wildcards (* or ?).</param>
    /// <returns>The base path without wildcard segments, or null if the input is null or whitespace.</returns>
    public static string? GetBasePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        path = Path.GetFullPath(path);
        if (EndsInDirectorySeparator(path))
            path += "*.*";
        var wildCards = new[] { '*', '?' };
        var pathSegments = path.Split('/', '\\');
        var segmentsQuery = from ps in pathSegments
                            select (segment: ps, hasWildcard: wildCards.Any(c => ps.Contains(c)));
        var basePath = string.Join(Path.DirectorySeparatorChar, segmentsQuery.TakeWhile(ps => !ps.hasWildcard).Select(ps => ps.segment));

        return path == basePath ? Path.GetDirectoryName(basePath) : basePath;
    }

    /// <summary>
    /// Enumerates files matching a wildcard path pattern. The literal leading segments become the root and the rest is matched
    /// with the platform glob matcher: <c>*</c> and <c>?</c> within a segment, <c>**</c> across directories.
    /// </summary>
    /// <param name="wildcardPath">The path pattern to search for, which may contain wildcard characters.</param>
    /// <returns>An enumerable collection of full file paths matching the wildcard pattern.</returns>
    public static IEnumerable<string> EnumerateFiles(string? wildcardPath)
    {
        if (string.IsNullOrWhiteSpace(wildcardPath))
            return [];

        wildcardPath = Path.GetFullPath(wildcardPath);

        if (File.Exists(wildcardPath))
            return [wildcardPath];

        if (EndsInDirectorySeparator(wildcardPath))
            wildcardPath += "*";
        var segments = wildcardPath.Split('/', '\\');
        var literal = segments.TakeWhile(s => s.IndexOfAny(_wildCards) < 0).ToArray();
        var pattern = segments.Skip(literal.Length).ToArray();
        if (pattern.Length == 0)
            return [];

        return EnumerateFiles(string.Join(Path.DirectorySeparatorChar, literal), [string.Join('/', pattern)]);
    }

    /// <summary>
    /// Enumerates files under a root that match glob patterns.
    /// </summary>
    /// <param name="root">The directory the patterns are relative to; nothing outside it is matched.</param>
    /// <param name="patterns">Glob patterns such as <c>**/*.cs</c>. A pattern starting with <c>!</c> excludes what it matches, for example <c>!**/obj/**</c>.</param>
    /// <returns>The full paths of the matching files; empty when the root does not exist.</returns>
    public static IEnumerable<string> EnumerateFiles(string root, IEnumerable<string> patterns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(patterns);
        if (!Directory.Exists(root))
            return [];

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        var singleCharacter = new List<Regex>();
        foreach (var pattern in patterns)
        {
            if (pattern.StartsWith('!'))
            {
                matcher.AddExclude(pattern[1..]);
                continue;
            }

            matcher.AddInclude(pattern.Replace('?', '*'));
            singleCharacter.Add(ToRegex(pattern));
        }

        var fullRoot = Path.GetFullPath(root);
        var results = matcher.GetResultsInFullPath(fullRoot);
        if (!patterns.Any(p => !p.StartsWith('!') && p.Contains('?')))
            return results;

        // The matcher has no single-character wildcard: it matched with '?' widened to '*', so narrow the candidates again.
        return results.Where(file => singleCharacter.Exists(r => r.IsMatch(Path.GetRelativePath(fullRoot, file).Replace(Path.DirectorySeparatorChar, '/')))).ToList();
    }

    private static Regex ToRegex(string pattern)
    {
        var body = new StringBuilder();
        var segments = pattern.Replace(Path.DirectorySeparatorChar, '/').Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            var last = i == segments.Length - 1;
            if (segments[i] == "**")
            {
                body.Append(last ? ".*" : "(?:.*/)?");
                continue;
            }

            foreach (var c in segments[i])
                body.Append(c switch { '*' => "[^/]*", '?' => "[^/]", _ => Regex.Escape(c.ToString()) });
            if (!last)
                body.Append('/');
        }

        return new Regex($"^{body}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Enumerates directories matching a wildcard path pattern.
    /// Supports hierarchical wildcard patterns with asterisks.
    /// </summary>
    /// <param name="path">The base path to start searching from.</param>
    /// <param name="wildCardPath">The wildcard pattern for matching directories (e.g., "**" for recursive).</param>
    /// <returns>Enumerable of directory paths matching the pattern.</returns>
    public static IEnumerable<string> EnumerateDirectories(string path, string wildCardPath)
    {
        if (string.IsNullOrWhiteSpace(wildCardPath))
        {
            if (Directory.Exists(path))
            {
                yield return path;
            }
            yield break;
        }
        path = Path.GetFullPath(path);
        var wildCards = new[] { '*', '?' };
        var pathSegments = wildCardPath.Split('/', '\\');
        var segmentsQuery = from ps in pathSegments
                            select (segment: ps, hasWildcard: wildCards.Any(c => ps.Contains(c)));

        var basePath = Path.Combine(path, string.Join(Path.DirectorySeparatorChar, segmentsQuery.TakeWhile(ps => !ps.hasWildcard).Select(ps => ps.segment)));
        var searchPathSegments = segmentsQuery.SkipWhile(ps => !ps.hasWildcard);
        var enumerator = searchPathSegments.Select(s => s.segment).GetEnumerator();

        var directories = EnumerateDirectories(path, enumerator);
        foreach (var directory in directories)
            yield return directory;
    }

    internal static IEnumerable<string> EnumerateDirectories(string path, IEnumerator<string> enumerator)
    {
        IEnumerable<string>? directories = null;
        var recursive = false;
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            if (enumerator.Current == "**")
            {
                recursive = true;
                while (enumerator.MoveNext() && enumerator.Current == "**") { }
            }
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if (directories == null)
            {
                var matches = Directory.EnumerateDirectories(path, enumerator.Current ?? "*.*", searchOption);
                directories = matches;
            }
            else
            {
                var searchPath = enumerator.Current ?? "*.*";
                directories = from dir in directories
                              from child in Directory.EnumerateDirectories(dir, searchPath, searchOption)
                              select child;
            }

            recursive = false;
        }

        foreach (var dir in directories ?? [])
            yield return dir;
    }
}
