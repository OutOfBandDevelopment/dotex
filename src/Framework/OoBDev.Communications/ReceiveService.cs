using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Verifies webhook calls, tracks delivery status and dispatches events to the handlers.</summary>
public sealed partial class ReceiveService(
    IChannelRepository channels,
    IWebhookProcessorFactory processors,
    IDeliveryTracker tracker,
    IEnumerable<IMessageReceivedHandler> messageHandlers,
    IEnumerable<IDeliveryStatusHandler> statusHandlers,
    IOptions<SendReceiveOptions> sendReceiveOptions,
    TimeProvider timeProvider,
    ILogger<ReceiveService> logger) : IReceiveService
{
    /// <inheritdoc />
    public async Task<WebhookResult> HandleWebhookAsync(string channelName, WebhookRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelName);
        ArgumentNullException.ThrowIfNull(request);

        var channel = await channels.GetByNameAsync(channelName, cancellationToken).ConfigureAwait(false);
        if (channel is null)
        {
            return Fail($"Channel '{channelName}' not found");
        }

        var processor = processors.GetProcessor(channel.Provider);
        if (processor is null)
        {
            return Fail($"Webhook processor not found for provider '{channel.Provider}'");
        }

        var age = timeProvider.GetUtcNow() - request.ReceivedAt;
        if (age.Duration() > sendReceiveOptions.Value.WebhookClockTolerance)
        {
            return Fail("Webhook is outside the clock tolerance");
        }

        if (!processor.ValidateSignature(channel, request))
        {
            return Fail("Webhook signature is not valid");
        }

        var events = await processor.ProcessAsync(channel, request, cancellationToken).ConfigureAwait(false);
        foreach (var evt in events)
        {
            switch (evt)
            {
                case MessageReceivedEvent received:
                    foreach (var handler in messageHandlers)
                    {
                        await RunAsync(evt.MessageId, () => handler.HandleAsync(received.Message, cancellationToken)).ConfigureAwait(false);
                    }

                    break;
                case DeliveryStatusEvent status:
                    await tracker.UpdateStatusAsync(status.Update, cancellationToken).ConfigureAwait(false);
                    foreach (var handler in statusHandlers)
                    {
                        await RunAsync(evt.MessageId, () => handler.HandleAsync(status.Update, cancellationToken)).ConfigureAwait(false);
                    }

                    break;
            }
        }

        return new WebhookResult(true, events.Count, events.Select(e => e.MessageId).ToList());
    }

    private static WebhookResult Fail(string message) => new(false, 0, [], message);

    private async Task RunAsync(string messageId, Func<Task> handler)
    {
        try
        {
            await handler().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogHandlerFailed(ex, messageId);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "A handler failed for message {MessageId}; the other handlers still ran.")]
    private partial void LogHandlerFailed(Exception exception, string messageId);
}
