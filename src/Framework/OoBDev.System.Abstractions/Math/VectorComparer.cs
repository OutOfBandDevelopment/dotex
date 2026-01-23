using System.Collections;
using System.Collections.Generic;

namespace OoBDev.System.Math;

/// <summary>
/// Provides comparison functionality for vectors using configurable distance metrics.
/// </summary>
public class VectorComparer : IComparer<Vector>, IComparer<double[]>, IComparer
{
    /// <summary>
    /// Gets or initializes the distance metric used for comparing vectors. Default is <see cref="VectorDistanceMetrics.Cosine"/>.
    /// </summary>
    public VectorDistanceMetrics DistanceMetric { get; init; } = VectorDistanceMetrics.Cosine;

    /// <summary>
    /// Compares two <see cref="Vector"/> instances.
    /// </summary>
    /// <param name="x">The first vector to compare.</param>
    /// <param name="y">The second vector to compare.</param>
    /// <returns>A value less than zero if x is closer to the origin, zero if equal distance, or greater than zero if y is closer.</returns>
    public int Compare(Vector x, Vector y) => Compare((object?)x, (object?)y);

    /// <summary>
    /// Compares two double arrays as vectors.
    /// </summary>
    /// <param name="x">The first vector to compare.</param>
    /// <param name="y">The second vector to compare.</param>
    /// <returns>A value less than zero if x is closer to the origin, zero if equal distance, or greater than zero if y is closer.</returns>
    public int Compare(double[]? x, double[]? y) => Compare((object?)x, (object?)y);

    /// <summary>
    /// Compares two objects as vectors.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A value less than zero if x is closer to the origin, zero if equal distance, or greater than zero if y is closer.</returns>
    public int Compare(object? x, object? y)
    {
        var vector1 = Vector.From(x);
        var vector2 = Vector.From(y);

        if (vector1 == null && vector2 == null) return 0;

        var distance = VectorMath.Distance(vector1, vector2, DistanceMetric);

        if (distance < 0) return -1;
        else if (distance > 0) return 1;
        else return 0;
    }
}
