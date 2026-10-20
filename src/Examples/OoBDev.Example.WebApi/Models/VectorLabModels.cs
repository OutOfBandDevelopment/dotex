using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>Vectors and the metric to compare them with.</summary>
public class VectorMatrixRequestModel
{
    /// <summary>Gets or sets the vectors, one list of numbers each.</summary>
    public required List<List<double>> Vectors { get; set; }

    /// <summary>Gets or sets the metric: Cosine, Euclidean, DotProduct or Manhattan.</summary>
    public string Metric { get; set; } = "Cosine";
}

/// <summary>Pairwise distances, magnitudes and centroid.</summary>
public class VectorMatrixResponseModel
{
    /// <summary>Gets or sets the distance between every pair of vectors.</summary>
    public required List<List<double>> Matrix { get; set; }

    /// <summary>Gets or sets the length of each vector.</summary>
    public required List<double> Magnitudes { get; set; }

    /// <summary>Gets or sets the mean of the vectors.</summary>
    public required List<double> Centroid { get; set; }
}
