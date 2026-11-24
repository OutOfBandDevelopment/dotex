using System.Collections.Generic;

namespace OoBDev.Imaging.People;

/// <summary>
/// The outcome of one clustering run.
/// </summary>
/// <param name="Assignments">One entry per item that was unclustered; items that already had a cluster are never listed or changed.</param>
/// <param name="NewClusterSpecies">The species of each new cluster in order (entry k-1 belongs to target <c>-k</c>); null entries for faces.</param>
public sealed record ClusterResult(IReadOnlyList<ClusterAssignment> Assignments, IReadOnlyList<string?> NewClusterSpecies);
