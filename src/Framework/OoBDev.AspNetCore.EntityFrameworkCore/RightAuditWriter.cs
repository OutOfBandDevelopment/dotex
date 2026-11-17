using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>Drains the buffer into the database in batches and purges entries past their retention.</summary>
public sealed partial class RightAuditWriter(
    BufferedRightAuditSink sink,
    EfRightAuditStore store,
    IOptions<RightAuditOptions> options,
    TimeProvider timeProvider,
    ILogger<RightAuditWriter> logger) : BackgroundService
{
    private DateTimeOffset lastPurge = DateTimeOffset.MinValue;

    /// <summary>Writes everything buffered now and purges when a day has passed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of entries written.</returns>
    public async Task<int> FlushAsync(CancellationToken cancellationToken = default)
    {
        var written = 0;
        var batch = new List<RightAuditEntry>(options.Value.BatchSize);
        while (sink.Channel.Reader.TryRead(out var entry))
        {
            batch.Add(entry);
            if (batch.Count >= options.Value.BatchSize)
            {
                written += await WriteAsync(batch, cancellationToken).ConfigureAwait(false);
            }
        }

        written += await WriteAsync(batch, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        if (now - lastPurge >= TimeSpan.FromDays(1))
        {
            lastPurge = now;
            var purged = await store.PurgeAsync(now.AddDays(-options.Value.RetentionDays), cancellationToken).ConfigureAwait(false);
            if (purged > 0)
            {
                LogPurged(purged);
            }
        }

        return written;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.FlushSeconds), timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await FlushAsync(stoppingToken).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // the writer must outlive a database fault
                catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
                {
                    LogFailed(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down; the final flush below keeps what is buffered.
        }

        await FlushAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task<int> WriteAsync(List<RightAuditEntry> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return 0;
        }

        var count = batch.Count;
        await store.AppendAsync(batch, cancellationToken).ConfigureAwait(false);
        batch.Clear();
        return count;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Right audit: purged {Count} entries past retention")]
    private partial void LogPurged(int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Right audit: writing a batch failed")]
    private partial void LogFailed(Exception exception);
}
