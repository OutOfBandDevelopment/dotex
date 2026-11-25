using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>A deferral store that survives restarts and is shared by every instance; each held request is handed out once.</summary>
/// <param name="factory">Creates a context per call.</param>
public sealed class EfDeferralStore(IDbContextFactory<OrchestrationDbContext> factory) : IDeferralStore
{
    /// <inheritdoc />
    public async Task AddAsync(DeferredRequest deferred, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deferred);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        db.DeferredRequests.Add(new DeferredRequestRow
        {
            Id = Guid.NewGuid(),
            DueAtTicks = deferred.DueAt.UtcTicks,
            TargetId = deferred.Request.TargetId,
            MessageType = deferred.Request.MessageType,
            Priority = (int)deferred.Request.Priority,
            CorrelationId = deferred.Request.CorrelationId,
            DataJson = RequestDataJson.Write(deferred.Request.Data),
        });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeferredRequest>> TakeDueAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var cutoff = now.UtcTicks;
        var due = await db.DeferredRequests.AsNoTracking().Where(r => r.DueAtTicks <= cutoff).OrderBy(r => r.DueAtTicks).ToListAsync(cancellationToken).ConfigureAwait(false);

        var taken = new List<DeferredRequest>(due.Count);
        foreach (var row in due)
        {
            // Another instance may have taken the row first; only the one whose delete counts owns it.
            var removed = await db.DeferredRequests.Where(r => r.Id == row.Id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            if (removed == 1)
            {
                taken.Add(new DeferredRequest(
                    new CommunicationRequest(row.TargetId, row.MessageType, RequestDataJson.Read(row.DataJson), (RequestPriority)row.Priority, row.CorrelationId),
                    new DateTimeOffset(row.DueAtTicks, TimeSpan.Zero)));
            }
        }

        return taken;
    }
}
