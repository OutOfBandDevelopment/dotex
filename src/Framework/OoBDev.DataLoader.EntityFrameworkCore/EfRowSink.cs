using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.DataLoader.EntityFrameworkCore;

/// <summary>Stores rows in a database table by dataset and one key column.</summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="dataset">The dataset name.</param>
/// <param name="keyColumn">The column that identifies a row.</param>
public sealed class EfRowSink(IDbContextFactory<LoadedRowsDbContext> factory, string dataset, string keyColumn) : IBatchRowSink
{
    /// <inheritdoc />
    public async ValueTask<WriteResult> WriteAsync(RowContext row, DuplicatePolicy policy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(row);
        var key = row.Values.TryGetValue(keyColumn, out var k) ? k ?? string.Empty : string.Empty;
        var json = JsonSerializer.Serialize(row.Values);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var existing = await db.Rows.FindAsync([dataset, key], cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                db.Rows.Add(new LoadedRow { Dataset = dataset, Key = key, ValuesJson = json });
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return WriteResult.Written;
            }

            switch (policy)
            {
                case DuplicatePolicy.Replace:
                    existing.ValuesJson = json;
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return WriteResult.Replaced;
                case DuplicatePolicy.Skip:
                    return WriteResult.Skipped;
                default:
                    return WriteResult.Duplicate;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<WriteResult>> WriteBatchAsync(IReadOnlyList<RowContext> rows, DuplicatePolicy policy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var keys = rows.Select(r => r.Values.TryGetValue(keyColumn, out var k) ? k ?? string.Empty : string.Empty).ToArray();
        var results = new WriteResult[rows.Count];
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var distinct = keys.Distinct().ToArray();
            var tracked = await db.Rows.Where(x => x.Dataset == dataset && distinct.Contains(x.Key)).ToDictionaryAsync(x => x.Key, cancellationToken).ConfigureAwait(false);
            for (var i = 0; i < rows.Count; i++)
            {
                var json = JsonSerializer.Serialize(rows[i].Values);
                if (!tracked.TryGetValue(keys[i], out var existing))
                {
                    var added = new LoadedRow { Dataset = dataset, Key = keys[i], ValuesJson = json };
                    db.Rows.Add(added);
                    tracked[keys[i]] = added;
                    results[i] = WriteResult.Written;
                }
                else if (policy == DuplicatePolicy.Replace)
                {
                    existing.ValuesJson = json;
                    results[i] = WriteResult.Replaced;
                }
                else
                {
                    results[i] = policy == DuplicatePolicy.Skip ? WriteResult.Skipped : WriteResult.Duplicate;
                }
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return results;
    }

    /// <summary>Reads a stored row back.</summary>
    /// <param name="key">The key value.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The values, or null when absent.</returns>
    public async Task<IReadOnlyDictionary<string, string?>?> GetAsync(string key, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var row = await db.Rows.AsNoTracking().FirstOrDefaultAsync(x => x.Dataset == dataset && x.Key == key, cancellationToken).ConfigureAwait(false);
            return row is null ? null : JsonSerializer.Deserialize<Dictionary<string, string?>>(row.ValuesJson);
        }
    }
}
