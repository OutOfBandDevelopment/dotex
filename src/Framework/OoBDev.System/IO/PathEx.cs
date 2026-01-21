using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.System.IO;

/// <summary>
/// Extensions for path strings
/// </summary>
public static class PathEx
{
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
    /// Enumerates files matching a wildcard path pattern.
    /// Supports wildcards (* and ?) in any path segment and recursively searches directories.
    /// </summary>
    /// <param name="wildcardPath">The path pattern to search for, which may contain wildcard characters.</param>
    /// <returns>An enumerable collection of full file paths matching the wildcard pattern.</returns>
    public static IEnumerable<string> EnumerateFiles(string? wildcardPath)
    {
        if (string.IsNullOrWhiteSpace(wildcardPath))
            yield break;

        wildcardPath = Path.GetFullPath(wildcardPath);

        if (File.Exists(wildcardPath))
        {
            yield return wildcardPath;
            yield break;
        }

        if (EndsInDirectorySeparator(wildcardPath))
            wildcardPath += "*.*";
        var wildCards = new[] { '*', '?' };
        var pathSegments = wildcardPath.Split('/', '\\');
        var segmentsQuery = from ps in pathSegments
                            select (segment: ps, hasWildcard: wildCards.Any(c => ps.Contains(c)));
        var basePath = string.Join(Path.DirectorySeparatorChar, segmentsQuery.TakeWhile(ps => !ps.hasWildcard).Select(ps => ps.segment));
        var searchPathSegments = string.Join(Path.DirectorySeparatorChar, segmentsQuery.SkipWhile(ps => !ps.hasWildcard).Select(ps => ps.segment));
        var searchPaths = Path.GetDirectoryName(searchPathSegments);
        var searchFilePattern = Path.GetFileName(searchPathSegments);

        if (searchPaths != null)
            foreach (var directory in EnumerateDirectories(basePath, searchPaths))
                foreach (var file in Directory.EnumerateFiles(directory, searchFilePattern))
                    yield return file;
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
                while (enumerator.MoveNext() && enumerator.Current == "**")
                    ;
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
