using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>
/// Database <see cref="IScheduleStore"/>. A lease is taken with one conditional UPDATE, so two instances (or two stores
/// on the same database) never lease the same item.
/// </summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="time">Clock for lease expiry.</param>
/// <param name="maxAttempts">Leases allowed before an item is marked failed.</param>
public sealed class EfScheduleStore(IDbContextFactory<ScheduleDbContext> factory, TimeProvider time, int maxAttempts = 3) : IScheduleStore
{
    /// <inheritdoc />
    public async Task<ScheduleItem> RegisterAsync(string key, DateTimeOffset dueAt, string eventType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        await using var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var row = await db.Schedule.FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new ScheduleRow { Id = Guid.NewGuid(), Key = key };
            db.Schedule.Add(row);
        }

        row.DueAtTicks = dueAt.UtcTicks;
        row.EventType = eventType;
        row.Payload = payload.ToArray();
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToItem(row);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScheduleItem>> LeaseDueAsync(DateTimeOffset now, int max, TimeSpan lease, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var clock = time.GetUtcNow().UtcTicks;
        var until = clock + lease.Ticks;
        var nowTicks = now.UtcTicks;

        await db.Schedule
            .Where(x => !x.Failed && x.Attempts >= maxAttempts && x.DueAtTicks <= nowTicks && x.LeasedUntilTicks <= clock)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Failed, true), cancellationToken).ConfigureAwait(false);

        var candidates = await db.Schedule.AsNoTracking()
            .Where(x => !x.Failed && x.DueAtTicks <= nowTicks && x.LeasedUntilTicks <= clock)
            .OrderBy(x => x.DueAtTicks).Select(x => x.Id).Take(max)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var leased = new List<ScheduleItem>();
        foreach (var id in candidates)
        {
            var taken = await db.Schedule
                .Where(x => x.Id == id && !x.Failed && x.LeasedUntilTicks <= clock)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.LeasedUntilTicks, until).SetProperty(x => x.Attempts, x => x.Attempts + 1), cancellationToken)
                .ConfigureAwait(false);
            if (taken != 1) continue;

            var row = await db.Schedule.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
            leased.Add(ToItem(row));
        }

        return leased;
    }

    /// <inheritdoc />
    public async Task CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await db.Schedule.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await db.Schedule.Where(x => x.Key == key).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <summary>Items marked failed.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    public async Task<IReadOnlyList<ScheduleItem>> FailedItemsAsync(CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Schedule.AsNoTracking().Where(x => x.Failed).ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Select(ToItem).ToList();
    }

    private static ScheduleItem ToItem(ScheduleRow r)
        => new(r.Id, r.Key, new DateTimeOffset(r.DueAtTicks, TimeSpan.Zero), r.EventType, r.Payload, r.Attempts, r.Failed);
}
