using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>Keeps last-run times in the schedule database so catch-up survives a restart.</summary>
/// <param name="factory">Creates a context per call.</param>
public sealed class EfLastRunStore(IDbContextFactory<ScheduleDbContext> factory) : ILastRunStore
{
    /// <inheritdoc />
    public async Task<DateTimeOffset?> GetAsync(string key, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var row = await db.LastRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
            return row is null ? null : new DateTimeOffset(row.LastRunTicks, TimeSpan.Zero);
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, DateTimeOffset lastRun, CancellationToken cancellationToken)
    {
        var ticks = lastRun.UtcTicks;
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var row = await db.LastRuns.FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
            if (row is null)
            {
                db.LastRuns.Add(new LastRunRow { Key = key, LastRunTicks = ticks });
            }
            else if (ticks > row.LastRunTicks)
            {
                row.LastRunTicks = ticks;
            }
            else
            {
                return;
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
