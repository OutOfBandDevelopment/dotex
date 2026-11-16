using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OoBDev.WorkQueue;

/// <summary>
/// Hosted service that claims items of one stage, runs the stage handler with a heartbeat and records the outcome.
/// </summary>
public sealed partial class StageWorker : BackgroundService
{
    private readonly string _stage;
    private readonly IWorkQueue _queue;
    private readonly IWorkStageHandler _handler;
    private readonly WorkQueueOptions _options;
    private readonly TimeProvider _clock;
    private readonly ILogger _logger;
    private readonly string _worker;

    /// <summary>Creates the worker.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="queue">The queue.</param>
    /// <param name="handler">The stage handler.</param>
    /// <param name="options">The work queue options.</param>
    /// <param name="clock">The time source.</param>
    /// <param name="logger">The logger.</param>
    public StageWorker(string stage, IWorkQueue queue, IWorkStageHandler handler, IOptions<WorkQueueOptions> options, TimeProvider clock, ILogger<StageWorker> logger)
    {
        _stage = stage;
        _queue = queue;
        _handler = handler;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
        _worker = $"{Environment.MachineName}:{Environment.ProcessId}:{stage}";
    }

    /// <summary>Claims and works one batch.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How many items were claimed.</returns>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var items = await _queue.ClaimAsync(_stage, _worker, _options.BatchSize, cancellationToken);
        foreach (var item in items)
            await WorkAsync(item, cancellationToken);
        return items.Count;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.For(_stage).Enabled)
        {
            LogDisabled(_stage);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var claimed = 0;
            try
            {
                claimed = await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogLoopFailed(ex, _stage);
            }

            if (claimed == 0)
                await Task.Delay(_options.PollInterval, _clock, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task WorkAsync(WorkItem item, CancellationToken cancellationToken)
    {
        using var lost = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var beat = HeartbeatAsync(item, lost);
        try
        {
            await _handler.HandleAsync(item, lost.Token);
            await lost.CancelAsync();
            await beat;
            if (!await _queue.CompleteAsync(item, cancellationToken: cancellationToken))
                LogLeaseLost(_stage, item.Key);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            await lost.CancelAsync();
            await beat;
            var retryable = ex is not NonRetryableWorkException;
            var status = await _queue.FailAsync(item, ex.Message, retryable, cancellationToken);
            LogFailed(ex, _stage, item.Key, item.Attempt, status ?? "lease lost");
        }
    }

    private async Task HeartbeatAsync(WorkItem item, CancellationTokenSource lost)
    {
        var interval = _options.HeartbeatInterval ?? _options.For(item.Stage).Lease / 3;
        try
        {
            while (!lost.IsCancellationRequested)
            {
                await Task.Delay(interval, _clock, lost.Token);
                if (!await _queue.HeartbeatAsync(item, lost.Token))
                {
                    LogLeaseLost(_stage, item.Key);
                    await lost.CancelAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // the handler finished or the worker is stopping
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Stage {Stage} is disabled; no worker runs")]
    private partial void LogDisabled(string stage);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Stage {Stage} worker loop failed")]
    private partial void LogLoopFailed(Exception exception, string stage);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Stage {Stage} item {Key} lost its lease")]
    private partial void LogLeaseLost(string stage, string key);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Stage {Stage} item {Key} failed attempt {Attempt}: now {Status}")]
    private partial void LogFailed(Exception exception, string stage, string key, int attempt, string status);
}
