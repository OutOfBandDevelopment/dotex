#pragma warning disable MA0048 // related small types share a file
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.IO;
using System.Linq;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// The script a request maps to.
/// </summary>
/// <param name="ScriptName">The script file name (the first path segment).</param>
/// <param name="ScriptPath">The full path of the script.</param>
/// <param name="PathInfo">The rest of the path, or an empty string.</param>
/// <param name="Executable">The handler executable, or an empty string to run the script directly.</param>
public record CgiScript(string ScriptName, string ScriptPath, string PathInfo, string Executable);

/// <summary>
/// Resolves a request path to a script.
/// </summary>
public interface ICgiPathResolver
{
    /// <summary>
    /// Resolves the path that follows the prefix.
    /// </summary>
    /// <param name="remainingPath">The path after the prefix, starting with a slash.</param>
    /// <returns>The script.</returns>
    /// <exception cref="CgiException">403 or 404 when the path is refused or nothing matches.</exception>
    CgiScript Resolve(string remainingPath);
}

/// <summary>
/// Resolves paths under <see cref="CgiOptions.ScriptRoot"/> and refuses anything that leaves it.
/// </summary>
public class CgiPathResolver(IOptions<CgiOptions> options) : ICgiPathResolver
{
    private static readonly SearchValues<char> _invalidName = SearchValues.Create(['/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0']);

    /// <inheritdoc />
    public CgiScript Resolve(string remainingPath)
    {
        var settings = options.Value;
        var trimmed = (remainingPath ?? string.Empty).TrimStart('/');
        if (trimmed.Length == 0)
            throw new CgiException(404, "No script was named.");

        var slash = trimmed.IndexOf('/', StringComparison.Ordinal);
        var name = slash < 0 ? trimmed : trimmed[..slash];
        var pathInfo = slash < 0 ? string.Empty : trimmed[slash..];

        if (name is "." or ".." || name.AsSpan().IndexOfAny(_invalidName) >= 0 || pathInfo.Contains('\0', StringComparison.Ordinal))
            throw new CgiException(403, "The script name is not allowed.");

        var extension = Path.GetExtension(name);
        if (!settings.Handlers.TryGetValue(extension, out var executable))
            throw new CgiException(403, "This script type is not allowed.");
        if (!settings.AllowBatchFiles && extension.ToUpperInvariant() is ".CMD" or ".BAT")
            throw new CgiException(403, "Batch files are not allowed.");

        var root = Path.GetFullPath(settings.ScriptRoot);
        var scriptPath = Path.GetFullPath(Path.Combine(root, name));
        if (!IsInside(root, scriptPath))
            throw new CgiException(403, "The script is outside the script root.");
        if (!File.Exists(scriptPath))
            throw new CgiException(404, "The script was not found.");

        var link = new FileInfo(scriptPath).ResolveLinkTarget(returnFinalTarget: true);
        if (link != null && !IsInside(root, link.FullName))
            throw new CgiException(403, "The script is outside the script root.");

        return new CgiScript(name, scriptPath, pathInfo, executable ?? string.Empty);
    }

    private static bool IsInside(string root, string path)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, comparison) && path.Length > prefix.Length;
    }
}
