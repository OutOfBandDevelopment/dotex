using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Imaging.People;

/// <summary>
/// Thresholds, weights and the schedule policy of the people and pet clustering.
/// </summary>
public sealed class PeopleClusteringOptions
{
    /// <summary>Gets or sets the cosine similarity a face needs to join an existing cluster.</summary>
    [Range(0.0, 1.0)]
    public double FaceThreshold { get; set; } = 0.40;

    /// <summary>Gets or sets the cosine similarity a pet needs to join an existing cluster of its species.</summary>
    [Range(0.0, 1.0)]
    public double PetThreshold { get; set; } = 0.55;

    /// <summary>Gets or sets the higher similarity a pet needs to join a cluster the owner has named.</summary>
    [Range(0.0, 1.0)]
    public double PetNamedThreshold { get; set; } = 0.65;

    /// <summary>Gets or sets how much more a member the owner assigned by hand weighs in a pet centroid.</summary>
    [Range(1.0, 100.0)]
    public double ConfirmedWeight { get; set; } = 5.0;

    /// <summary>Gets or sets the least time between two clustering runs of one kind.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    public TimeSpan MinInterval { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Gets or sets how many unclustered items must wait before a run is worth starting.</summary>
    [Range(1, int.MaxValue)]
    public int MinNewItems { get; set; } = 25;
}
