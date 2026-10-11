using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OoBDev.Data.Vectors.Hosting;

/// <summary>
/// Hosted service that manages the lifecycle of the embedding sentence transformer queue reader.
/// Handles automatic restarts on errors and graceful shutdown.
/// </summary>
public partial class EmbeddingSentenceTransformerQueueReaderHost : IHostedService, IDisposable
{
    private readonly IEmbeddingSentenceTransformerQueueReader _reader;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _tokenSource = new();

    private Task? _task;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingSentenceTransformerQueueReaderHost"/> class.
    /// </summary>
    /// <param name="reader">The queue reader to host.</param>
    /// <param name="logger">The logger for diagnostics.</param>
    public EmbeddingSentenceTransformerQueueReaderHost(
        IEmbeddingSentenceTransformerQueueReader reader,
        ILogger<EmbeddingSentenceTransformerQueueReaderHost> logger
        )
    {
        _reader = reader;
        _logger = logger;
    }

    private bool _disposed;

    /// <summary>
    /// Disposes resources used by this host.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by this host.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            LogRequestDispose();
            _tokenSource.Cancel();
            LogCompleteDispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Starts the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_task != null)
        {
            throw new InvalidOperationException($"Already started!");
        }

        LogRequestStart();
        var token = _tokenSource.Token;

        _task = Task.Run(async () =>
        {

            while (!token.IsCancellationRequested)
            {
                var reader = _reader;
                try
                {
                    LogStarting(reader);
                    var task =  reader.RunAsync(token);
                    LogStarted(reader);
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogException(reader, ex.Message);
                    LogError(reader, ex);

                    LogWaitingForRestart(reader);
                    await Task.Delay(10000, token).ConfigureAwait(false); // TODO: this should be configurable
                }
            }
        }, cancellationToken);

        LogCompletedStart();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogRequestStop();

        if (_task == null)
        {
            LogNotRunning();
            return;
        }

        await _tokenSource.CancelAsync().ConfigureAwait(false);
        await _task.ConfigureAwait(false);
        _task = null;

        LogCompletedStop();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Request Dispose")]
    private partial void LogRequestDispose();

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Complete Dispose")]
    private partial void LogCompleteDispose();

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Request Start")]
    private partial void LogRequestStart();

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Starting: {reader}")]
    private partial void LogStarting(IEmbeddingSentenceTransformerQueueReader reader);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Started: {reader}")]
    private partial void LogStarted(IEmbeddingSentenceTransformerQueueReader reader);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Exception: {reader}: {exception}")]
    private partial void LogException(IEmbeddingSentenceTransformerQueueReader reader, string exception);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Failed: {reader}")]
    private partial void LogError(IEmbeddingSentenceTransformerQueueReader reader, Exception exception);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Waiting for restart: {reader}")]
    private partial void LogWaitingForRestart(IEmbeddingSentenceTransformerQueueReader reader);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Completed Start")]
    private partial void LogCompletedStart();

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Request Stop")]
    private partial void LogRequestStop();

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Not Running")]
    private partial void LogNotRunning();

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Completed Stop")]
    private partial void LogCompletedStop();
}
