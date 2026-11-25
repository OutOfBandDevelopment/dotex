namespace OoBDev.Imaging.People;

/// <summary>
/// A candidate item close to a matched item.
/// </summary>
/// <param name="Id">The candidate identifier.</param>
/// <param name="Cluster">The candidate cluster; null when it has none.</param>
/// <param name="Similarity">The cosine similarity to the matched item.</param>
public sealed record NearCandidate(long Id, int? Cluster, double Similarity);
