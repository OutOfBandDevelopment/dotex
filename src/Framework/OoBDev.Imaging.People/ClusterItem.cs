namespace OoBDev.Imaging.People;

/// <summary>
/// A detected face or pet with its embedding.
/// </summary>
/// <param name="Id">The caller's identifier of the item.</param>
/// <param name="Embedding">The embedding vector.</param>
/// <param name="Cluster">The cluster the item already belongs to, or null when it is still unclustered.</param>
/// <param name="Species">The species of a pet; null for a face. Pets only match their own species.</param>
/// <param name="Confirmed">Whether the owner assigned the item to its cluster by hand (it then weighs more in a pet centroid).</param>
public sealed record ClusterItem(long Id, float[] Embedding, int? Cluster, string? Species = null, bool Confirmed = false);
