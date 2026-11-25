using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Wraps a handler so a message id is handled once per consumer. A message already recorded is skipped; a handler
/// that throws releases the id so the broker's redelivery is handled again (at-least-once delivery, effectively-once handling).
/// </summary>
/// <param name="consumer">The consumer name that scopes the recorded ids.</param>
/// <param name="inner">The handler to protect.</param>
/// <param name="inbox">Where handled ids are recorded.</param>
public class IdempotentMessageQueueHandler(string consumer, IMessageQueueHandler inner, IInboxStore inbox) : IMessageQueueHandler
{
    /// <inheritdoc />
    public async Task HandleAsync(object message, IMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var id = context.SentId ?? context.OriginMessageId;
        if (string.IsNullOrEmpty(id))
        {
            await inner.HandleAsync(message, context).ConfigureAwait(false);
            return;
        }

        if (!await inbox.TryBeginAsync(consumer, id).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            await inner.HandleAsync(message, context).ConfigureAwait(false);
        }
        catch
        {
            await inbox.ReleaseAsync(consumer, id).ConfigureAwait(false);
            throw;
        }
    }
}
