namespace OoBDev.Data.Vectors.Hosting;

/// <summary>
/// Configuration options for the embedding sentence transformer queue reader.
/// </summary>
public class EmbeddingSentenceTransformerQueueReaderOptions
{
    /// <summary>
    /// Configuration section name for these options.
    /// </summary>
    public const string ConfigSection = nameof(EmbeddingSentenceTransformerQueueReaderOptions);

    /// <summary>
    /// Gets or initializes the maximum number of messages to read from the queue in a single batch.
    /// Default is 100.
    /// </summary>
    public int MaximumReadLength { get; init; } = 100;

    /// <summary>
    /// Gets or initializes the timeout duration when waiting for messages from the queue.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan ReadWaitTimeout { get; init; } = new(0, 5, 0);

    /// <summary>
    /// Gets or initializes the wait duration after an error before retrying.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan ErrorWaitTimeout { get; init; } = new(0, 0, 30);

    /// <summary>
    /// Gets or initializes how long the host waits before it restarts a reader that stopped with an error.
    /// Default is 10 seconds.
    /// </summary>
    public TimeSpan RestartDelay { get; init; } = TimeSpan.FromSeconds(10);
}
