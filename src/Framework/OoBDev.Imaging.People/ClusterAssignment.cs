namespace OoBDev.Imaging.People;

/// <summary>
/// The cluster chosen for one previously unclustered item.
/// </summary>
/// <param name="Id">The item identifier.</param>
/// <param name="Target">A zero or positive existing cluster id, or <c>-k</c> for the k-th new cluster of this run (1 based).</param>
/// <param name="Similarity">The cosine similarity to the chosen centroid; zero for the item that seeded a new cluster.</param>
/// <param name="Why">The reason in words: threshold met, named group, or seeded a new cluster.</param>
public sealed record ClusterAssignment(long Id, int Target, double Similarity, string Why);
