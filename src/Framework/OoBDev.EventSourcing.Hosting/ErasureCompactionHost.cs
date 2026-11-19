using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Hosting;

/// <summary>
/// Runs <see cref="ErasureService.CompactAsync"/> on a timer so a recorded erasure request is completed without an operator command.
/// Every pass uses its own scope; a failed pass is logged and tried again after the poll interval.
/// </summary>
/// <param name="scopeFactory">Creates the scope of one pass.</param>
/// <param name="options">The poll interval.</param>
/// <param name="logger">The logger.</param>
public partial class ErasureCompactionHost(
    IServiceScopeFactory scopeFactory,
    IOptions<ErasureJobOptions> options,
    ILogger<ErasureCompactionHost> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var completed = await scope.ServiceProvider.GetRequiredService<ErasureService>().CompactAsync(stoppingToken).ConfigureAwait(false);
                if (completed > 0)
                    LogCompleted(completed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogPassFailed(ex);
            }

            try
            {
                await Task.Delay(options.Value.PollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Completed {Count} erasure request(s)")]
    private partial void LogCompleted(int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Erasure compaction pass failed; trying again after the poll interval")]
    private partial void LogPassFailed(Exception exception);
}
