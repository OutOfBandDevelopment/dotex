using Microsoft.EntityFrameworkCore;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>Writes, reads and purges the durable right audit trail.</summary>
/// <param name="factory">Creates a context per call.</param>
public sealed class EfRightAuditStore(IDbContextFactory<RightAuditDbContext> factory) : IRightAuditQuery
{
    /// <summary>Stores a batch in one transaction.</summary>
    /// <param name="entries">The checks.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AppendAsync(IReadOnlyList<RightAuditEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0)
        {
            return;
        }

        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        db.RightAudit.AddRange(entries.Select(e => new RightAuditRow
        {
            AtTicks = e.At.UtcTicks,
            Subject = Clip(e.Subject, 200),
            Rights = Clip(e.Rights, 1000),
            Result = Clip(e.Result, 50),
        }));
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads the trail, newest first.</summary>
    /// <param name="subject">Only this caller, or all when <see langword="null"/>.</param>
    /// <param name="from">Only checks at or after this time.</param>
    /// <param name="maxCount">The most entries to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<RightAuditEntry>> QueryAsync(string? subject = null, DateTimeOffset? from = null, int maxCount = 100, CancellationToken cancellationToken = default) =>
        QueryAsync(new RightAuditFilter(subject, null, from, null, maxCount), cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RightAuditEntry>> QueryAsync(RightAuditFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentOutOfRangeException.ThrowIfLessThan(filter.MaxCount, 1, nameof(filter));
        var (subject, from, maxCount) = (filter.Subject, filter.From, filter.MaxCount);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var query = db.RightAudit.AsNoTracking();
        if (subject is not null)
        {
            query = query.Where(x => x.Subject == subject);
        }

        if (from is { } start)
        {
            var ticks = start.UtcTicks;
            query = query.Where(x => x.AtTicks >= ticks);
        }

        if (filter.To is { } end)
        {
            var endTicks = end.UtcTicks;
            query = query.Where(x => x.AtTicks < endTicks);
        }

        if (filter.Result is { } result)
        {
            query = query.Where(x => x.Result == result);
        }

        var rows = await query.OrderByDescending(x => x.AtTicks).ThenByDescending(x => x.Id).Take(maxCount).ToListAsync(cancellationToken).ConfigureAwait(false);
        return [.. rows.Select(r => new RightAuditEntry(new DateTimeOffset(r.AtTicks, TimeSpan.Zero), r.Subject, r.Rights, r.Result))];
    }

    /// <summary>Deletes entries older than the cutoff.</summary>
    /// <param name="olderThan">The cutoff.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of entries deleted.</returns>
    public async Task<int> PurgeAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
    {
        var ticks = olderThan.UtcTicks;
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        return await db.RightAudit.Where(x => x.AtTicks < ticks).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string Clip(string value, int length) => value.Length <= length ? value : value[..length];
}
