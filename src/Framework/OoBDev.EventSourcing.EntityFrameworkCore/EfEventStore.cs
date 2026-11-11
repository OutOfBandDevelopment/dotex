using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// Event store on Entity Framework Core. The unique index on (StreamId, Sequence) decides concurrent writers.
/// </summary>
public sealed class EfEventStore(IDbContextFactory<EventSourcingDbContext> factory, TimeProvider timeProvider) : IEventStore
{
    /// <inheritdoc />
    public async Task<AppendResult> AppendAsync(string streamId, long expectedSequence, IReadOnlyList<NewEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamId);
        ArgumentNullException.ThrowIfNull(events);

        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var last = await LastSequenceAsync(db, streamId, cancellationToken).ConfigureAwait(false);
        if (last != expectedSequence)
            return new AppendResult(false, last, []);

        var now = timeProvider.GetUtcNow();
        var rows = events
            .Select((item, index) => new EventRow
            {
                StreamId = streamId,
                Sequence = expectedSequence + index + 1,
                Type = item.Type,
                Timestamp = now,
                Payload = item.Payload,
                Metadata = item.Metadata,
            })
            .ToList();
        db.Events.AddRange(rows);

        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var current = await LastSequenceAsync(db, streamId, cancellationToken).ConfigureAwait(false);
            if (current == expectedSequence)
                throw;
            return new AppendResult(false, current, []);
        }

        return new AppendResult(true, rows.Count == 0 ? expectedSequence : rows[^1].Sequence, [.. rows.Select(ToRecord)]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventRecord>> ReadAsync(string streamId, long fromSequence = 1, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var rows = await db.Events.AsNoTracking()
            .Where(e => e.StreamId == streamId && e.Sequence >= fromSequence)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return [.. rows.Select(ToRecord)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventRecord>> ReadAllAsync(long afterPosition, int maxCount = 500, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var rows = await db.Events.AsNoTracking()
            .Where(e => e.Position > afterPosition)
            .OrderBy(e => e.Position)
            .Take(maxCount)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return [.. rows.Select(ToRecord)];
    }

    private static async Task<long> LastSequenceAsync(EventSourcingDbContext db, string streamId, CancellationToken cancellationToken)
        => await db.Events.Where(e => e.StreamId == streamId).MaxAsync(e => (long?)e.Sequence, cancellationToken).ConfigureAwait(false) ?? 0;

    private static EventRecord ToRecord(EventRow row)
        => new(row.StreamId, row.Sequence, row.Type, row.Timestamp, row.Payload, row.Metadata, row.Position);
}
