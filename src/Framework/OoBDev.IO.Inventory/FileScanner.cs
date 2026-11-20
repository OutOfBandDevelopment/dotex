using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.IO.Inventory;

/// <summary>Walks a root read-only and records size, time and a fast content hash; unchanged files are not re-read.</summary>
/// <param name="store">Where entries are kept.</param>
public sealed class FileScanner(IInventoryStore store)
{
    private readonly IInventoryStore _store = store ?? throw new ArgumentNullException(nameof(store));

    /// <summary>Scans a root folder.</summary>
    /// <param name="root">Folder to walk.</param>
    /// <param name="followLinks">Follow symbolic links (off by default, loop safety).</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The counts and any unreadable files.</returns>
    public async Task<ScanReport> ScanAsync(string root, bool followLinks, CancellationToken cancellationToken)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = followLinks ? FileAttributes.None : FileAttributes.ReparsePoint,
        };
        var scanned = 0;
        var hashed = 0;
        var skipped = new List<(string Path, string Reason)>();
        foreach (var path in Directory.EnumerateFiles(root, "*", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanned++;
            try
            {
                var info = new FileInfo(path);
                var modified = new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero);
                var known = await _store.FindAsync(path, cancellationToken).ConfigureAwait(false);
                if (known is not null && known.Length == info.Length && known.Modified == modified) continue;

                var hash = await FastHashAsync(path, cancellationToken).ConfigureAwait(false);
                await _store.SaveAsync(new InventoryEntry(path, info.Length, modified, hash), cancellationToken).ConfigureAwait(false);
                hashed++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                skipped.Add((path, ex.Message));
            }
        }

        return new ScanReport(scanned, hashed, skipped);
    }

    private static async Task<string> FastHashAsync(string path, CancellationToken cancellationToken)
    {
        var hasher = new XxHash3();
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await hasher.AppendAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hasher.GetCurrentHash());
    }
}
