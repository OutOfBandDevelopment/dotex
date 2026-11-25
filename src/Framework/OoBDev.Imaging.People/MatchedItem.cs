using System.Collections.Generic;

namespace OoBDev.Imaging.People;

/// <summary>
/// One item of a match response, filled as far as the stages that ran.
/// </summary>
public sealed record MatchedItem
{
    /// <summary>Gets the detection.</summary>
    public required DetectedItem Detection { get; init; }

    /// <summary>Gets the embedding; null until the embed stage ran, and left out of a response unless <see cref="MatchRequest.IncludeEmbeddings"/> is set.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Gets the cluster: zero or positive for an existing cluster, <c>-k</c> for the k-th new cluster of its kind in this run, null when the cluster stage did not run.</summary>
    public int? Cluster { get; init; }

    /// <summary>Gets the cosine similarity to the chosen centroid; zero for an item that seeded a cluster.</summary>
    public double? Similarity { get; init; }

    /// <summary>Gets the reason in words (threshold met, named group, seeded a new cluster).</summary>
    public string? Why { get; init; }

    /// <summary>Gets the closest candidates, best first.</summary>
    public IReadOnlyList<NearCandidate> Nearest { get; init; } = [];
}
