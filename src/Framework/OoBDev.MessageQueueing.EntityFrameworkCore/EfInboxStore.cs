using Microsoft.EntityFrameworkCore;
using OoBDev.MessageQueueing.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>
/// Relational <see cref="IInboxStore"/>: the primary key <c>(Consumer, MessageId)</c> makes the first insert the winner, so two
/// processes cannot both begin the same message.
/// </summary>
/// <param name="factory">Creates a context per call.</param>
public sealed class EfInboxStore(IDbContextFactory<MessagingDbContext> factory) : IInboxStore
{
    /// <inheritdoc />
    public async Task<bool> TryBeginAsync(string consumer, string messageId)
    {
        ArgumentException.ThrowIfNullOrEmpty(consumer);
        ArgumentException.ThrowIfNullOrEmpty(messageId);
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var dbDisposer = db.ConfigureAwait(false);
        db.Inbox.Add(new InboxRow { Consumer = consumer, MessageId = messageId });
        try
        {
            await db.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException)
        {
            // A lost race or an earlier insert; a real connection failure surfaces on the next call.
            var check = await factory.CreateDbContextAsync().ConfigureAwait(false);
            await using var checkDisposer = check.ConfigureAwait(false);
            if (await check.Inbox.AnyAsync(x => x.Consumer == consumer && x.MessageId == messageId).ConfigureAwait(false))
            {
                return false;
            }

            throw;
        }
    }

    /// <inheritdoc />
    public async Task ReleaseAsync(string consumer, string messageId)
    {
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var dbDisposer = db.ConfigureAwait(false);
        await db.Inbox.Where(x => x.Consumer == consumer && x.MessageId == messageId).ExecuteDeleteAsync().ConfigureAwait(false);
    }
}
