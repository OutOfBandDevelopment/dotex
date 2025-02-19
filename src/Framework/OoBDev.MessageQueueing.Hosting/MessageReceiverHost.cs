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
public class MessageReceiverHost(
    ILogger<MessageReceiverHost> logger,
    IMessageReceiverProviderFactory factory
    ) : IHostedService, IDisposable
{
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _tokenSource = new();

    private bool _disposed = false;

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
            logger.LogInformation("Request Dispose");
            _tokenSource.Cancel();
            logger.LogInformation("Complete Dispose");
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

        logger.LogInformation("Request Start");

        var providers = factory.Create().ToArray();

        var token = _tokenSource.Token;

        foreach (var provider in providers)
        {
            _tasks.Add(Task.Run(() => StartProvider(provider, token), cancellationToken));
        }

        logger.LogInformation("Completed Start");

        return Task.CompletedTask;
    }

    private async Task StartProvider(IMessageReceiverProvider provider, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation($"Starting: {{{nameof(provider)}}}", provider);
                var task = provider.RunAsync(token);
                logger.LogInformation($"Started: {{{nameof(provider)}}}", provider);
                await task;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {{{nameof(provider)}}}: {{{nameof(Exception)}}}", provider, ex.Message);
                logger.LogDebug($"Error: {{{nameof(provider)}}}: {{{nameof(Exception)}}}", provider, ex.ToString());

                logger.LogInformation($"Waiting for restart: {{{nameof(provider)}}}", provider);
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
        logger.LogInformation("Request Stop");

        await _tokenSource.CancelAsync();
        await Task.WhenAll(_tasks);
        _tasks.Clear();

        logger.LogInformation("Completed Stop");
    }
}
