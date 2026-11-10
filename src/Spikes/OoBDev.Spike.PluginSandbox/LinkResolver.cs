using System;
using System.IO;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>Resolves symbolic links, junctions and mount-point links in every part of a path, so a link inside a granted folder cannot lead the broker outside it.</summary>
public static class LinkResolver
{
    private const int MaxLinks = 32;

    /// <summary>
    /// The full path with every existing link followed. Parts that do not exist yet (a file about to be written) are kept as written,
    /// after the real location of the deepest existing parent.
    /// </summary>
    public static string Real(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var full = Path.GetFullPath(path);
        for (var round = 0; round < MaxLinks; round++)
        {
            var next = Step(full);
            if (string.Equals(next, full, StringComparison.Ordinal))
            {
                return full;
            }

            full = next;
        }

        throw new IOException("Too many levels of links in " + path);
    }

    private static string Step(string full)
    {
        var root = Path.GetPathRoot(full) ?? string.Empty;
        var current = root;
        var rest = full[root.Length..].Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < rest.Length; i++)
        {
            current = Path.Combine(current, rest[i]);
            FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
            if (!info.Exists || info.LinkTarget is null)
            {
                continue;
            }

            var target = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? current;
            for (var j = i + 1; j < rest.Length; j++)
            {
                target = Path.Combine(target, rest[j]);
            }

            return Path.GetFullPath(target);
        }

        return full;
    }
}
