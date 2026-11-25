using System.Collections.Generic;

namespace OoBDev.Imaging.People;

/// <summary>
/// One match run: what to look at, which kinds and stages, and the set to match against.
/// </summary>
public sealed record MatchRequest
{
    /// <summary>Gets the encoded image; needed by the detect stage and by the embed stage.</summary>
    public byte[]? Image { get; init; }

    /// <summary>Gets the kinds to look for in the detect stage.</summary>
    public MatchKinds Kinds { get; init; } = MatchKinds.Both;

    /// <summary>Gets the pet species to keep from the detector; empty keeps all.</summary>
    public IReadOnlySet<string> Species { get; init; } = new HashSet<string>();

    /// <summary>Gets the stages to run.</summary>
    public MatchStages Stages { get; init; } = MatchStages.All;

    /// <summary>Gets items from an earlier run to continue with; they are added to what the detect stage finds.</summary>
    public IReadOnlyList<MatchedItem> Items { get; init; } = [];

    /// <summary>Gets the known faces and pets the cluster stage matches against. Items that already have a cluster, owner-confirmed ones included, never change.</summary>
    public IReadOnlyList<ClusterItem> Candidates { get; init; } = [];

    /// <summary>Gets the ids of pet clusters the owner has named.</summary>
    public IReadOnlySet<int> NamedPetClusters { get; init; } = new HashSet<int>();

    /// <summary>Gets the thresholds; null uses the defaults.</summary>
    public PeopleClusteringOptions? Options { get; init; }

    /// <summary>Gets a value indicating whether the response carries the embeddings (they are large and private).</summary>
    public bool IncludeEmbeddings { get; init; }

    /// <summary>Gets how many nearest candidates each item lists.</summary>
    public int NearestCount { get; init; } = 3;
}
