namespace OoBDev.SBert;

/// <summary>
/// Options for configuring SBert.
/// </summary>
public class SentenceEmbeddingOptions
{
    /// <summary>
    /// Gets or sets the URL for SBert.
    /// </summary>
    /// <remarks>
    /// Example: http://sbert.example.com:5080
    /// </remarks>
    public required string Url { get; set; }
}
