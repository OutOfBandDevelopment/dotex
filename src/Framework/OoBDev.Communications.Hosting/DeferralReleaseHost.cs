using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Hosting;

/// <summary>Calls <see cref="ICommunicationOrchestrator.ReleaseDueAsync"/> every <see cref="SendReceiveOptions.DeferralReleaseInterval"/> so held requests go out when they are due.</summary>
public sealed partial class DeferralReleaseHost(
    ICommunicationOrchestrator orchestrator,
    IOptions<SendReceiveOptions> options,
    TimeProvider timeProvider,
    ILogger<DeferralReleaseHost> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.DeferralReleaseInterval, timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    var released = await orchestrator.ReleaseDueAsync(stoppingToken).ConfigureAwait(false);
                    if (released > 0)
                    {
                        LogReleased(released);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogReleaseFailed(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Released {Count} deferred communication request(s).")]
    private partial void LogReleased(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Releasing deferred communication requests failed; it is retried on the next interval.")]
    private partial void LogReleaseFailed(Exception exception);
}
