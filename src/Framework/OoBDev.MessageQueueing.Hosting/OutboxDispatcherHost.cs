using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.MessageQueueing.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Hosting;

/// <summary>Runs <see cref="OutboxDispatcher"/> on a timer so outbox entries are published without the caller doing it.</summary>
public partial class OutboxDispatcherHost(
    OutboxDispatcher dispatcher,
    IOptions<OutboxDispatcherHostOptions> options,
    ILogger<OutboxDispatcherHost> logger) : BackgroundService
{
    private readonly OutboxDispatcherHostOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var sent = 0;
            try
            {
                sent = await dispatcher.DispatchAsync(_options.BatchSize).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogDispatchFailed(ex);
            }

            if (sent == 0)
            {
                try
                {
                    await Task.Delay(_options.PollInterval, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox dispatch failed; retrying after the poll interval")]
    private partial void LogDispatchFailed(Exception ex);
}
