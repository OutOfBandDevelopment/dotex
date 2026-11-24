using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Documents.Packaging;

namespace OoBDev.Documents;

/// <summary>Reads a folder into <see cref="PackageItem"/>s so it can be packed with <see cref="IDocumentPackager"/>.</summary>
public static class DirectoryPackageItems
{
    /// <summary>Reads every file under <paramref name="directory"/>, with entry names relative to it (forward slashes, ordinal order).</summary>
    /// <param name="directory">The folder to read.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist.</exception>
    public static async Task<IReadOnlyCollection<PackageItem>> ReadAsync(string directory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var root = Path.GetFullPath(directory);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"The folder '{directory}' does not exist.");
        }

        var items = new List<PackageItem>();
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
        {
            var name = Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/');
            items.Add(new PackageItem(name, await File.ReadAllBytesAsync(file, cancellationToken).ConfigureAwait(false)));
        }

        return items;
    }
}
