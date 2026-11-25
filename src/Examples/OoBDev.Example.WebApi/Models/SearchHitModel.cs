namespace OoBDev.WebApi.Models;

/// <summary>A passage found by one search mode.</summary>
public class SearchHitModel
{
    /// <summary>Gets or sets the passage name.</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the score of the mode (BM25, cosine similarity or fused rank score).</summary>
    public double Score { get; set; }

    /// <summary>Gets or sets the passage text.</summary>
    public required string Text { get; set; }
}
