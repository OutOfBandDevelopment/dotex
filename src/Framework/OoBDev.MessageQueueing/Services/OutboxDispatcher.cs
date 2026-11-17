using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Publishes pending outbox entries through the real sender. An entry is marked sent only after the send
/// succeeds, so a crash in between repeats the send (at-least-once); pair consumers with <see cref="IdempotentMessageQueueHandler"/>.
/// </summary>
/// <param name="outbox">Where messages wait.</param>
/// <param name="sender">The real sender.</param>
public class OutboxDispatcher(IOutboxStore outbox, IMessageQueueSender sender)
{
    /// <summary>Sends up to <paramref name="batchSize"/> pending entries. A failed send stops the batch, so order is kept.</summary>
    /// <param name="batchSize">The most entries to send.</param>
    /// <returns>How many were sent.</returns>
    public async Task<int> DispatchAsync(int batchSize = 100)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);
        var sent = 0;
        foreach (var entry in await outbox.GetPendingAsync(batchSize).ConfigureAwait(false))
        {
            await sender.SendAsync(entry.Message, entry.Id).ConfigureAwait(false);
            await outbox.MarkSentAsync(entry.Id).ConfigureAwait(false);
            sent++;
        }

        return sent;
    }
}
