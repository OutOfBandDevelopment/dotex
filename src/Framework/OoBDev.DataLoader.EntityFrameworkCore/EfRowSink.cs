using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.DataLoader.EntityFrameworkCore;

/// <summary>Stores rows in a database table by dataset and one key column.</summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="dataset">The dataset name.</param>
/// <param name="keyColumn">The column that identifies a row.</param>
public sealed class EfRowSink(IDbContextFactory<LoadedRowsDbContext> factory, string dataset, string keyColumn) : IRowSink
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
