using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OoBDev.Data.Vectors.Hosting;

public class EmbeddingSentenceTransformerQueueReaderHost : IHostedService, IDisposable
{
    private readonly IEmbeddingSentenceTransformerQueueReader _reader;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _tokenSource = new();

    private Task? _task;

    public EmbeddingSentenceTransformerQueueReaderHost(
        IEmbeddingSentenceTransformerQueueReader reader,
        ILogger<EmbeddingSentenceTransformerQueueReaderHost> logger
        )
    {
        _reader = reader;
        _logger = logger;
    }

    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _logger.LogInformation("Request Dispose");
            _tokenSource.Cancel();
            _logger.LogInformation("Complete Dispose");
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

        _logger.LogInformation("Request Start");
        var token = _tokenSource.Token;

        _task = Task.Run(async () =>
        {

            while (!token.IsCancellationRequested)
            {
                var reader = _reader;
                try
                {
                    _logger.LogInformation($"Starting: {{{nameof(reader)}}}", reader);
                    var task =  reader.RunAsync(token);
                    _logger.LogInformation($"Started: {{{nameof(reader)}}}", reader);
                    await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception: {{{nameof(reader)}}}: {{{nameof(Exception)}}}", reader, ex.Message);
                    _logger.LogDebug($"Error: {{{nameof(reader)}}}: {{{nameof(Exception)}}}", reader, ex.ToString());

                    _logger.LogInformation($"Waiting for restart: {{{nameof(reader)}}}", reader);
                    await Task.Delay(10000, token); // TODO: this should be configurable
                }
            }
        }, cancellationToken);

        _logger.LogInformation("Completed Start");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request Stop");

        if (_task == null)
        {
            _logger.LogInformation("Not Running");
            return;
        }

        await _tokenSource.CancelAsync();
        await _task;
        _task = null;

        _logger.LogInformation("Completed Stop");
    }

}
