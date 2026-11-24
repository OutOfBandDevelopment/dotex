using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.Documents.Packaging;

/// <summary>Writes a ZIP archive (no password: encryption is not part of the framework ZIP support).</summary>
public sealed class ZipDocumentPackageHandler : IDocumentPackageHandler
{
    /// <inheritdoc />
    public string PackageType => "zip";

    /// <inheritdoc />
    public async Task PackAsync(IReadOnlyCollection<PackageItem> items, Stream destination, CancellationToken cancellationToken = default)
    {
        using var archive = new ZipArchive(destination, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var item in items)
        {
            var entry = archive.CreateEntry(item.EntryName, CompressionLevel.Optimal);
            var stream = entry.Open();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteAsync(item.Content, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
