using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Sends messages through channels with idempotency, retry and backoff.</summary>
public sealed class SendService(
    IChannelRepository channels,
    IChannelRegistry registry,
    IDeliveryTracker tracker,
    IOptions<SendReceiveOptions> sendReceiveOptions,
    TimeProvider timeProvider) : ISendService
{
    /// <inheritdoc />
    public async Task<SendResult> SendAsync(string channelName, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelName);
        ArgumentNullException.ThrowIfNull(message);

        var channel = await channels.GetByNameAsync(channelName, cancellationToken).ConfigureAwait(false)
            ?? throw new ChannelNotFoundException(channelName);
        var provider = registry.GetProvider(channel.Protocol, channel.Provider)
            ?? throw new ProviderNotFoundException(channel.Protocol, channel.Provider);

        var earlier = await tracker.GetSentResultAsync(message.MessageId, channelName, cancellationToken).ConfigureAwait(false);
        if (earlier is { Success: true })
        {
            return earlier;
        }

        var settings = sendReceiveOptions.Value;
        var retries = options?.RetryCount ?? settings.RetryCount;
        var baseDelay = options?.RetryDelay ?? settings.RetryDelay;
        var correlationId = message.CorrelationId ?? options?.CorrelationId;

        SendResult result;
        var attempt = 0;
        while (true)
        {
            result = await provider.SendAsync(channel, message, cancellationToken).ConfigureAwait(false);
            if (result.CorrelationId is null && correlationId is not null)
            {
                result = result with { CorrelationId = correlationId };
            }

            if (result.Success || !IsRetryable(result.Failure) || attempt >= retries)
            {
                break;
            }

            await Task.Delay(NextDelay(baseDelay, attempt, result.Failure, settings), timeProvider, cancellationToken).ConfigureAwait(false);
            attempt++;
        }

        await tracker.RecordSentAsync(message.MessageId, channelName, result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SendResult>> SendToMultipleChannelsAsync(IReadOnlyCollection<string> channelNames, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channelNames);
        var tasks = channelNames.Select(name => SendAsync(name, message, options, cancellationToken)).ToArray();
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static bool IsRetryable(SendFailure? failure)
        => failure is { Kind: SendFailureKind.Transient or SendFailureKind.RateLimited };

    private static TimeSpan NextDelay(TimeSpan baseDelay, int attempt, SendFailure? failure, SendReceiveOptions settings)
    {
        var ticks = baseDelay.Ticks * Math.Pow(settings.BackoffFactor, attempt);
        var delay = ticks >= settings.MaxRetryDelay.Ticks ? settings.MaxRetryDelay : TimeSpan.FromTicks((long)ticks);
        return failure?.RetryAfter is { } asked && asked > delay ? asked : delay;
    }
}
