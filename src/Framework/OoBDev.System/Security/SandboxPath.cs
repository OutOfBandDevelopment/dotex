using OoBDev.System.IO;
using System;
using System.IO;

namespace OoBDev.System.Security;

/// <summary>
/// File IO sandboxing operation
/// </summary>
public static class SandboxPath
{
    /// <summary>
    /// Resolves <c>filePath</c> against <c>basePath</c> (an absolute path stays as given) and returns it only when it is
    /// the sandbox folder or inside it, so a request cannot leave the sandbox with <c>..</c> or an absolute path elsewhere.
    /// </summary>
    /// <param name="basePath">The sandbox folder; relative paths are resolved against it, not the current directory.</param>
    /// <param name="filePath">The requested path.</param>
    /// <returns>The full path inside the sandbox.</returns>
    /// <exception cref="InvalidOperationException">The path is outside the sandbox.</exception>
    public static string? EnsureSafePath(string basePath, string filePath)
    {
        var sandbox = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var composedPath = Path.GetFullPath(filePath, sandbox + Path.DirectorySeparatorChar);

        var inside = string.Equals(composedPath, sandbox, StringComparison.OrdinalIgnoreCase)
            || composedPath.StartsWith(sandbox + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        return inside
            ? PathEx.FixUpPath(composedPath)
            : throw new InvalidOperationException($"invalid path requested: {filePath}");
    }
    ///// <summary>
    ///// Ensure the path under <c>filePath</c> is a child of <c>basePath</c>.
    ///// </summary>
    ///// <param name="basePath"></param>
    ///// <param name="filePath"></param>
    ///// <returns></returns>
    ///// <exception cref="System.ApplicationException">this will be throw if <c>filePath</c> is not a child of <c>basePath</c>.</exception>
    //public static bool IsSafePath(string basePath, string filePath)
    //{
    //    var sandbox = Path.GetFullPath(basePath);
    //    var composedPath = Path.Combine(basePath, filePath);
    //    var fullPath = Path.GetFullPath(composedPath);
    //    return fullPath.StartsWith(sandbox);
    //}
}
