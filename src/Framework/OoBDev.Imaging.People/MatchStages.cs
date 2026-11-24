using System;

namespace OoBDev.Imaging.People;

/// <summary>
/// The stages of one match run. A run may do any subset, so the work can be split across hosts and resumed from the items of an earlier response.
/// </summary>
[Flags]
public enum MatchStages
{
    /// <summary>No stage.</summary>
    None = 0,

    /// <summary>Find faces and pets in the image.</summary>
    Detect = 1,

    /// <summary>Compute the embedding of each item that has none.</summary>
    Embed = 2,

    /// <summary>Assign each unclustered item to a cluster of the candidate set, or seed a new one.</summary>
    Cluster = 4,

    /// <summary>Every stage.</summary>
    All = Detect | Embed | Cluster,
}
