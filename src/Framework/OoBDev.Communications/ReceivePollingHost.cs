using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Polls every configured channel that sets <see cref="ChannelDefinition.PollInterval"/> through <see cref="IReceiveService.PollAsync"/>.</summary>
public sealed partial class ReceivePollingHost(
    IReceiveService receive,
    IOptions<SendReceiveOptions> options,
    TimeProvider timeProvider,
    ILogger<ReceivePollingHost> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loops = options.Value.Channels
            .Where(c => c.PollInterval is { } interval && interval > TimeSpan.Zero)
            .Select(c => LoopAsync(c, stoppingToken))
            .ToList();
        return Task.WhenAll(loops);
    }

    private async Task LoopAsync(ChannelDefinition channel, CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(channel.PollInterval!.Value, timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await receive.PollAsync(channel.Name, channel.PollBatchSize, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogPollFailed(ex, channel.Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Polling channel {Channel} failed; it is retried on the next interval.")]
    private partial void LogPollFailed(Exception exception, string channel);
}
