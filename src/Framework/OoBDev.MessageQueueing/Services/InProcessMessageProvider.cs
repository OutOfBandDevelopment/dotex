using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents an in-process message provider that implements both <see cref="IMessageSenderProvider"/> and <see cref="IMessageReceiverProvider"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InProcessMessageProvider"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class InProcessMessageProvider(
    ILogger<InProcessMessageProvider> logger
    ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private IMessageHandlerProvider? _handlerProvider;
    private static readonly ConcurrentQueue<WrappedQueueMessage> _queue = new();

    /// <summary>
    /// Gets the key associated with the in-process message provider.
    /// </summary>
    public const string MessageProviderKey = "in-process";

    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation and returning the correlation ID.</returns>
    public virtual Task<string?> SendAsync(object message, IMessageContext context)
    {
        var wrapped = new WrappedQueueMessage
        {
            ContentType = "application/json;",
            PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
            CorrelationId = context.CorrelationId ?? Guid.NewGuid().ToString(), //TODO: do this better such has having a id generation interface that can be replaced
            Payload = message,
            Properties = context.Headers,
        };
        _queue.Enqueue(wrapped);

        return Task.FromResult<string?>(wrapped.CorrelationId);
    }

    /// <summary>
    /// Sets the message handler provider.
    /// </summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance of the message receiver provider.</returns>
    public virtual IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider;
        return this;
    }

    /// <summary>
    /// Runs the in-process message provider asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var newCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken).Token;

        if (_handlerProvider == null)
        {
            throw new ApplicationException($"{nameof(IMessageHandlerProvider)} is not provided");
        }

        while (!newCancellationToken.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out var wrapped))
            {
                logger.LogInformation("Nothing received, waiting...");
                await Task.Delay(1000, cancellationToken);  // TODO: this should be configurable
                continue;
            }

            await _handlerProvider.HandleAsync(wrapped, wrapped.CorrelationId);

            logger.LogInformation($"Dequeue: {{CorrelationId}}", wrapped.CorrelationId);
        }
    }
}
