namespace OoBDev.Data.Vectors.Hosting;

/// <summary>
/// Interface for reading and processing embedding requests from a queue.
/// </summary>
public interface IEmbeddingSentenceTransformerQueueReader
{
    /// <summary>
    /// Runs the queue reader, continuously processing embedding requests until cancellation.
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    /// <returns>A task representing the queue reading operation.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}
