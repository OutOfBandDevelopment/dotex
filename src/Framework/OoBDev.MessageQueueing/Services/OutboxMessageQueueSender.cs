using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// An <see cref="IMessageQueueSender"/> that stores the message in the outbox instead of sending it. An
/// <see cref="OutboxDispatcher"/> publishes it afterwards, so a message is never sent for a change that rolled back.
/// </summary>
/// <param name="outbox">Where messages wait.</param>
/// <param name="timeProvider">The clock for the entry time.</param>
public class OutboxMessageQueueSender(IOutboxStore outbox, TimeProvider timeProvider) : IMessageQueueSender
{
    /// <inheritdoc />
    public async Task<string> SendAsync(object message, string? messageId = null, string? callerMember = null, string? callerFile = null, int callerLine = 0)
    {
        ArgumentNullException.ThrowIfNull(message);
        var id = messageId ?? Guid.NewGuid().ToString("N");
        await outbox.AddAsync(new OutboxEntry(id, message, timeProvider.GetUtcNow())).ConfigureAwait(false);
        return id;
    }
}
