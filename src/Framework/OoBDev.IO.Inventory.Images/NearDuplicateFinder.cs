using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OoBDev.IO.Inventory.Images;

/// <summary>Groups images whose perceptual hashes are within a distance; nothing is moved or deleted.</summary>
public sealed class NearDuplicateFinder
{
    private static readonly HashSet<string> _extensions = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    /// <summary>Groups already hashed images (single-linkage: A near B and B near C puts all three together).</summary>
    /// <param name="hashes">Path and hash pairs.</param>
    /// <param name="maxDistance">Largest differing-bit count that still counts as near (about 5 for resized copies).</param>
    /// <returns>Groups of two or more paths, each sorted.</returns>
    public IReadOnlyList<IReadOnlyList<string>> Group(IReadOnlyList<(string Path, long Hash)> hashes, int maxDistance)
    {
        ArgumentNullException.ThrowIfNull(hashes);
        var parent = Enumerable.Range(0, hashes.Count).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }

        for (var i = 0; i < hashes.Count; i++)
        {
            for (var j = i + 1; j < hashes.Count; j++)
            {
                if (ImageHasher.Distance(hashes[i].Hash, hashes[j].Hash) <= maxDistance) parent[Find(j)] = Find(i);
            }
        }

        return Enumerable.Range(0, hashes.Count)
            .GroupBy(Find)
            .Where(g => g.Count() > 1)
            .Select(g => (IReadOnlyList<string>)g.Select(i => hashes[i].Path).Order(StringComparer.OrdinalIgnoreCase).ToList())
            .OrderBy(g => g[0], StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Hashes every image under a folder and groups the near duplicates; unreadable files are skipped and returned.</summary>
    /// <param name="root">The folder.</param>
    /// <param name="maxDistance">Largest differing-bit count that still counts as near.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The groups and the skipped files with the reason.</returns>
    public (IReadOnlyList<IReadOnlyList<string>> Groups, IReadOnlyList<(string Path, string Reason)> Skipped) Scan(string root, int maxDistance, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        var hashes = new List<(string, long)>();
        var skipped = new List<(string, string)>();
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
        foreach (var path in Directory.EnumerateFiles(root, "*", options).Where(p => _extensions.Contains(Path.GetExtension(p))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                hashes.Add((path, ImageHasher.DHash(stream)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                skipped.Add((path, ex.Message));
            }
        }

        return (Group(hashes, maxDistance), skipped);
    }
}
