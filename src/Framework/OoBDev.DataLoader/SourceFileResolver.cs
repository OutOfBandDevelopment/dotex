using System;
using System.IO;

namespace OoBDev.DataLoader;

/// <summary>Resolves a file named in a data row against the folder of the data file, and refuses a path that leaves it.</summary>
/// <param name="baseDirectory">The folder of the data file.</param>
public sealed class SourceFileResolver(string baseDirectory)
{
    private readonly string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(baseDirectory));

    /// <summary>Resolves <paramref name="relativePath"/>.</summary>
    /// <param name="relativePath">The path from the row.</param>
    /// <param name="fullPath">The full path when it stays under the folder.</param>
    /// <param name="error">Why not, when it does not.</param>
    /// <returns>True when the file exists under the folder.</returns>
    public bool TryResolve(string relativePath, out string fullPath, out string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            error = $"'{relativePath}' is outside the data folder";
            return false;
        }

        if (!File.Exists(fullPath))
        {
            error = $"file '{relativePath}' was not found";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
