using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OoBDev.ComplexEvents.Hosting;

/// <summary>Polls the <see cref="IScheduleStore"/> and runs each due item through the registered callback.</summary>
public sealed partial class ScheduleHost(
    IScheduleStore store,
    Func<ScheduleItem, CancellationToken, Task> run,
    IOptions<ScheduleHostOptions> options,
    TimeProvider timeProvider,
    ILogger<ScheduleHost> logger,
    IScheduleFailureHandler? failureHandler = null,
    RecurringScheduler? recurring = null) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var o = options.Value;
        Func<ScheduleItem, CancellationToken, Task> step = recurring is null ? run : async (item, ct) =>
        {
            await run(item, ct).ConfigureAwait(false);
            await recurring.CompletedAsync(item, ct).ConfigureAwait(false);
        };
        var runner = new ScheduleRunner(store, timeProvider, step, o.Lease, failureHandler);
        if (recurring is not null)
        {
            await recurring.StartAsync(stoppingToken).ConfigureAwait(false);
        }

        using var timer = new PeriodicTimer(o.PollInterval, timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await runner.RunDueAsync(o.BatchSize, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogRunFailed(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Running due schedule items failed; it is retried on the next interval.")]
    private partial void LogRunFailed(Exception exception);
}
