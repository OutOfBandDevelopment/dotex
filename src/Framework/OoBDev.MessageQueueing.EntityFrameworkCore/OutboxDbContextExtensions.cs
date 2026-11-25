using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>Writes an outbox message through the caller's own <see cref="DbContext"/>.</summary>
public static class OutboxDbContextExtensions
{
    /// <summary>
    /// Adds an outbox row to the context. It is saved by the caller's next <c>SaveChanges</c>, so the business change and the
    /// message commit or roll back together; the dispatcher sends it afterwards. The context's model needs
    /// <see cref="OutboxModelExtensions.AddOutbox"/>.
    /// </summary>
    /// <param name="context">The business context.</param>
    /// <param name="types">The allowed message types.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="timeProvider">The clock; the system clock when omitted.</param>
    /// <param name="id">The message id; a new one when omitted.</param>
    /// <returns>The message id.</returns>
    public static string StageOutbox(this DbContext context, OutboxMessageTypes types, object message, TimeProvider? timeProvider = null, string? id = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(message);
        var messageId = id ?? Guid.NewGuid().ToString("N");
        context.Set<OutboxRow>().Add(new OutboxRow
        {
            Id = messageId,
            TypeName = types.RequireName(message),
            Json = JsonSerializer.Serialize(message, message.GetType()),
            CreatedAtTicks = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcTicks,
        });
        return messageId;
    }
}
