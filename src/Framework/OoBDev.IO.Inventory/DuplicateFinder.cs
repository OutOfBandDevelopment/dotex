using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.IO.Inventory;

/// <summary>Groups scanned files by size and fast hash, then confirms each group with SHA-512. Nothing is deleted or moved.</summary>
public sealed class DuplicateFinder
{
    /// <summary>Finds exact duplicate groups in a store.</summary>
    /// <param name="store">The scanned entries.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>Confirmed groups of two or more files.</returns>
    public async IAsyncEnumerable<DuplicateGroup> FindAsync(IInventoryStore store, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        var all = new List<InventoryEntry>();
        await foreach (var e in store.ListAsync(cancellationToken).ConfigureAwait(false)) all.Add(e);

        foreach (var candidate in all.GroupBy(e => (e.Length, e.FastHash)).Where(g => g.Count() > 1))
        {
            var confirmed = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var entry in candidate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string sha;
                try
                {
                    sha = entry.Sha512 ?? await Sha512Async(entry.Path, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    continue;
                }

                if (entry.Sha512 is null) await store.SaveAsync(entry with { Sha512 = sha }, cancellationToken).ConfigureAwait(false);
                if (!confirmed.TryGetValue(sha, out var list)) confirmed[sha] = list = [];
                list.Add(entry.Path);
            }

            foreach (var (sha, paths) in confirmed.Where(c => c.Value.Count > 1))
                yield return new DuplicateGroup(candidate.Key.Length, sha, paths);
        }
    }

    private static async Task<string> Sha512Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Convert.ToHexString(await SHA512.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
    }
}
