using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Hosting;

/// <summary>
/// Runs <see cref="EventRelay"/> in a loop for the life of the host: batches back to back while events are waiting,
/// otherwise a pause of <see cref="EventRelayOptions.PollInterval"/>. A failed pass is logged and tried again after the pause.
/// </summary>
/// <param name="relay">The relay.</param>
/// <param name="options">The relay options.</param>
/// <param name="logger">The logger.</param>
public partial class EventRelayHost(
    EventRelay relay,
    IOptions<EventRelayOptions> options,
    ILogger<EventRelayHost> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pause = options.Value.PollInterval;
        while (!stoppingToken.IsCancellationRequested)
        {
            var sent = 0;
            try
            {
                sent = await relay.RunOnceAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogRelayFailed(ex);
            }

            if (sent == 0)
            {
                try
                {
                    await Task.Delay(pause, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Event relay pass failed; trying again after the poll interval")]
    private partial void LogRelayFailed(Exception exception);
}
