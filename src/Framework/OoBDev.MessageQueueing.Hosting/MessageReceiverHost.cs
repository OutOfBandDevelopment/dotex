using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Hosting;

/// <summary>
/// Hosted service responsible for starting and stopping message receivers based on the configured providers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageReceiverHost"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
/// <param name="factory">The message receiver provider factory.</param>
public partial class MessageReceiverHost(
    ILogger<MessageReceiverHost> logger,
    IMessageReceiverProviderFactory factory
    ) : IHostedService, IDisposable
{
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _tokenSource = new();

    private bool _disposed;

    /// <summary>
    /// Disposes of the resources used by the <see cref="MessageReceiverHost"/>.
    /// </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="MessageReceiverHost"/>.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // note: dispose managed state (managed objects).
            LogRequestDispose();
            _tokenSource.Cancel();
            LogCompleteDispose();
        }

        // note: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // note: set large fields to null.

        _disposed = true;
    }

    /// <summary>
    /// Starts the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_tasks.Count > 0)
        {
            throw new InvalidOperationException($"Already started!");
        }

        LogRequestStart();

        var providers = factory.Create().ToArray();

        var token = _tokenSource.Token;

        foreach (var provider in providers)
        {
            _tasks.Add(Task.Run(() => StartProvider(provider, token), cancellationToken));
        }

        LogCompletedStart();

        return Task.CompletedTask;
    }

    private async Task StartProvider(IMessageReceiverProvider provider, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                LogStarting(provider);
                var task = provider.RunAsync(token);
                LogStarted(provider);
                await task;
            }
            catch (Exception ex)
            {
                LogException(provider, ex.Message);
                LogError(provider, ex);

                LogWaitingForRestart(provider);
                await Task.Delay(10000, token); // TODO: this should be configurable
            }
        }
    }

    /// <summary>
    /// Stops the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogRequestStop();

        await _tokenSource.CancelAsync();
        await Task.WhenAll(_tasks);
        _tasks.Clear();

        LogCompletedStop();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Request Dispose")]
    private partial void LogRequestDispose();

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Complete Dispose")]
    private partial void LogCompleteDispose();

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Request Start")]
    private partial void LogRequestStart();

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Completed Start")]
    private partial void LogCompletedStart();

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Starting: {provider}")]
    private partial void LogStarting(IMessageReceiverProvider provider);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Started: {provider}")]
    private partial void LogStarted(IMessageReceiverProvider provider);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Exception: {provider}: {exception}")]
    private partial void LogException(IMessageReceiverProvider provider, string exception);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Failed: {provider}")]
    private partial void LogError(IMessageReceiverProvider provider, Exception exception);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Waiting for restart: {provider}")]
    private partial void LogWaitingForRestart(IMessageReceiverProvider provider);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Request Stop")]
    private partial void LogRequestStop();

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Completed Stop")]
    private partial void LogCompletedStop();
}
