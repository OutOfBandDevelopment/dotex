using System.Collections.Generic;

namespace OoBDev.Imaging.People;

/// <summary>
/// The outcome of a match run.
/// </summary>
/// <param name="Items">The items, with the detail of the stages that ran.</param>
/// <param name="StagesRun">The stages that ran.</param>
/// <param name="NewFaceClusters">How many new face clusters the run seeded (clusters <c>-1</c> to <c>-n</c>).</param>
/// <param name="NewPetClusterSpecies">The species of each new pet cluster in order (entry k-1 belongs to cluster <c>-k</c>).</param>
public sealed record MatchResponse(IReadOnlyList<MatchedItem> Items, MatchStages StagesRun, int NewFaceClusters, IReadOnlyList<string?> NewPetClusterSpecies);
