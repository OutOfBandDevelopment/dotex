using System.ComponentModel.DataAnnotations;

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
    [Required]
    public required string Url { get; set; }
}
