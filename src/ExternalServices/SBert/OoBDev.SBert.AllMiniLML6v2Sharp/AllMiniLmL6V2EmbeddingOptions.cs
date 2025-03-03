namespace OoBDev.SBert.AllMiniLML6v2Sharp;

/// <summary>
/// Configuration options for the AllMiniLmL6V2 embedding provider.
/// </summary>
public class AllMiniLmL6V2EmbeddingOptions
{
    /// <summary>
    /// The configuration prefix used for settings related to this embedding provider.
    /// </summary>
    public const string ConfigPrefix = "AllMiniLmL6V2Embedding";

    /// <summary>
    /// Gets or sets the percentage of parallelism to use for embedding generation.
    /// The value should be between 0 and 1, representing the fraction of available processing power to utilize.
    /// </summary>
    public double PercentageOfParallelism { get; set; } = 0.75;
}
