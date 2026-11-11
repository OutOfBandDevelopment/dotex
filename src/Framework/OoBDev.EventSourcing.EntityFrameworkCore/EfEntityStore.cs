using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// Entity store on Entity Framework Core with a version gate: updates and deletes are single conditional statements.
/// </summary>
public sealed class EfEntityStore(IDbContextFactory<EventSourcingDbContext> factory) : IEntityStore
{
    /// <inheritdoc />
    public async Task<EntityRecord?> GetAsync(string type, string key, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var row = await db.Entities.AsNoTracking().FirstOrDefaultAsync(e => e.Type == type && e.Key == key, cancellationToken).ConfigureAwait(false);
        return row is null ? null : ToRecord(row);
    }

    /// <inheritdoc />
    public async Task<EntityRecord?> SaveAsync(string type, string key, string value, long expectedVersion, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);

        if (expectedVersion == 0)
        {
            db.Entities.Add(new EntityRow { Type = type, Key = key, Version = 1, Value = value });
            try
            {
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                if (await db.Entities.AnyAsync(e => e.Type == type && e.Key == key, cancellationToken).ConfigureAwait(false))
                    return null;
                throw;
            }
            return new EntityRecord(type, key, 1, value);
        }

        var updated = await db.Entities
            .Where(e => e.Type == type && e.Key == key && e.Version == expectedVersion)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Value, value).SetProperty(e => e.Version, expectedVersion + 1), cancellationToken)
            .ConfigureAwait(false);
        return updated == 0 ? null : new EntityRecord(type, key, expectedVersion + 1, value);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string type, string key, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var query = db.Entities.Where(e => e.Type == type && e.Key == key);
        if (expectedVersion is not null)
            query = query.Where(e => e.Version == expectedVersion);
        return await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EntityRecord>> QueryAsync(string type, string keyPrefix = "", CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var rows = await db.Entities.AsNoTracking()
            .Where(e => e.Type == type && e.Key.StartsWith(keyPrefix))
            .OrderBy(e => e.Key)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return [.. rows.Select(ToRecord)];
    }

    private static EntityRecord ToRecord(EntityRow row) => new(row.Type, row.Key, row.Version, row.Value);
}
