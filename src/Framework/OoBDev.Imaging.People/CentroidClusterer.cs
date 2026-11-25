using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Imaging.People;

/// <summary>
/// Greedy centroid clustering of new faces and pets. Items that already have a cluster are fixed inputs and are never reassigned, so an assignment the owner confirmed cannot change.
/// </summary>
public static class CentroidClusterer
{
    /// <summary>
    /// Assigns each unclustered face, in the order given (best first), to the closest centroid at or above the threshold, or seeds a new cluster.
    /// </summary>
    /// <param name="items">The faces, clustered and unclustered.</param>
    /// <param name="options">The thresholds; <see cref="PeopleClusteringOptions.FaceThreshold"/> is used.</param>
    /// <returns>The assignments of the unclustered faces.</returns>
    public static ClusterResult Faces(IReadOnlyList<ClusterItem> items, PeopleClusteringOptions options)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(options);
        var centroids = new List<(int Target, double[] Sum)>();
        var slotOf = new Dictionary<int, int>();
        var unit = items.Select(i => Normalize(i.Embedding)).ToList();
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Cluster is not { } cluster)
                continue;
            if (!slotOf.TryGetValue(cluster, out var slot))
            {
                slotOf[cluster] = slot = centroids.Count;
                centroids.Add((cluster, new double[unit[i].Length]));
            }

            Add(centroids[slot].Sum, unit[i], 1);
        }

        // Ties go to the lowest cluster id, so the result does not depend on the order clusters were met.
        var order = centroids.Select((c, slot) => (c.Target, slot)).OrderBy(x => x.Target).Select(x => x.slot).ToList();
        var assignments = new List<ClusterAssignment>();
        var fresh = 0;
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Cluster is not null)
                continue;
            var best = -1;
            var bestSim = double.NegativeInfinity;
            foreach (var slot in order)
            {
                var sim = Dot(centroids[slot].Sum, unit[i]) / (Norm(centroids[slot].Sum) + 1e-9);
                if (sim > bestSim)
                {
                    bestSim = sim;
                    best = slot;
                }
            }

            string why;
            if (best < 0 || bestSim < options.FaceThreshold)
            {
                best = centroids.Count;
                centroids.Add((-(++fresh), new double[unit[i].Length]));
                order.Add(best);
                why = "seeded a new cluster: no centroid reached the threshold";
                bestSim = 0;
            }
            else
            {
                why = "threshold met";
            }

            Add(centroids[best].Sum, unit[i], 1);
            assignments.Add(new ClusterAssignment(items[i].Id, centroids[best].Target, bestSim, why));
        }

        return new ClusterResult(assignments, [.. Enumerable.Repeat<string?>(null, fresh)]);
    }

    /// <summary>
    /// Assigns each unclustered pet to the closest centroid of its own species at or above the threshold (a named cluster needs the higher named threshold), or seeds a new cluster.
    /// A member the owner assigned by hand weighs <see cref="PeopleClusteringOptions.ConfirmedWeight"/> in its centroid.
    /// </summary>
    /// <param name="items">The pets, clustered and unclustered; every item needs a species.</param>
    /// <param name="named">The ids of clusters the owner has named.</param>
    /// <param name="options">The thresholds and the confirmed weight.</param>
    /// <returns>The assignments of the unclustered pets and the species of each new cluster.</returns>
    public static ClusterResult Pets(IReadOnlyList<ClusterItem> items, ISet<int> named, PeopleClusteringOptions options)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(named);
        ArgumentNullException.ThrowIfNull(options);
        var centroids = new Dictionary<int, (double[] Sum, string Species)>();
        foreach (var item in items)
        {
            var species = item.Species ?? throw new ArgumentException("A pet needs a species.", nameof(items));
            if (item.Cluster is not { } cluster)
                continue;
            if (!centroids.TryGetValue(cluster, out var centroid))
                centroids[cluster] = centroid = (new double[item.Embedding.Length], species);
            Add(centroid.Sum, item.Embedding, item.Confirmed ? options.ConfirmedWeight : 1.0);
        }

        var assignments = new List<ClusterAssignment>();
        var freshSpecies = new List<string?>();
        foreach (var item in items)
        {
            if (item.Cluster is not null)
                continue;
            int? best = null;
            var bestSim = options.PetThreshold;
            var bestNamed = false;
            foreach (var (cluster, (sum, species)) in centroids)
            {
                if (!string.Equals(species, item.Species, StringComparison.Ordinal))
                    continue;
                var norm = Norm(sum);
                var sim = Dot(sum, item.Embedding) / (norm == 0 ? 1 : norm);
                var isNamed = named.Contains(cluster);
                if (sim >= bestSim && (!isNamed || sim >= Math.Max(options.PetThreshold, options.PetNamedThreshold)))
                {
                    best = cluster;
                    bestSim = sim;
                    bestNamed = isNamed;
                }
            }

            if (best is null)
            {
                freshSpecies.Add(item.Species);
                best = -freshSpecies.Count;
                centroids[best.Value] = (new double[item.Embedding.Length], item.Species!);
                assignments.Add(new ClusterAssignment(item.Id, best.Value, 0, "seeded a new cluster: no centroid of this species reached the threshold"));
            }
            else
            {
                assignments.Add(new ClusterAssignment(item.Id, best.Value, bestSim, bestNamed ? "named group, higher threshold met" : "threshold met"));
            }

            Add(centroids[best.Value].Sum, item.Embedding, 1.0);
        }

        return new ClusterResult(assignments, freshSpecies);
    }

    private static float[] Normalize(float[] v)
    {
        var n = (float)(Norm(v) + 1e-9);
        return [.. v.Select(x => x / n)];
    }

    private static void Add(double[] sum, float[] v, double weight)
    {
        for (var i = 0; i < sum.Length; i++)
            sum[i] += v[i] * weight;
    }

    private static double Dot(double[] a, float[] b)
    {
        double s = 0;
        for (var i = 0; i < a.Length; i++)
            s += a[i] * b[i];
        return s;
    }

    private static double Norm(double[] a) => Math.Sqrt(a.Sum(x => x * x));

    private static double Norm(float[] a)
    {
        double s = 0;
        foreach (var x in a)
            s += (double)x * x;
        return Math.Sqrt(s);
    }
}
