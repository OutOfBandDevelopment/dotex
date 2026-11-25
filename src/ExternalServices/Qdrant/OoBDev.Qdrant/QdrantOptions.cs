using Qdrant.Client.Grpc;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Qdrant;

/// <summary>
/// Configuration options for the Qdrant vector store.
/// </summary>
public class QdrantOptions
{
    /// <summary>
    /// Gets or sets the Qdrant gRPC address.
    /// </summary>
    [Required]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether storing vectors creates a missing collection.
    /// </summary>
    public bool EnsureCollectionExists { get; set; }

    /// <summary>
    /// Gets or sets the distance used when a collection is created (default cosine).
    /// </summary>
    public Distance Distance { get; set; } = Distance.Cosine;

    /// <summary>
    /// Gets or sets the most points a list or neighbour search returns (default 1000).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ResultLimit { get; set; } = 1000;
}
