using System;

namespace OoBDev.System.Math;

/// <summary>
/// Provides mathematical operations for vector calculations.
/// </summary>
public class VectorMath
{
    /// <summary>
    /// Calculates the distance between two vectors using the specified distance metric.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="distanceMetric">The distance metric to use. Default is <see cref="VectorDistanceMetrics.Cosine"/>.</param>
    /// <returns>The calculated distance, or null if either vector is null.</returns>
    public static double? Distance(Vector? vector1, Vector? vector2, VectorDistanceMetrics distanceMetric = VectorDistanceMetrics.Cosine)
    {
        if (vector1 == null || vector2 == null) return default;

        return distanceMetric switch
        {
            VectorDistanceMetrics.Cosine => CosineDistance(vector1.Value.Value, vector1.Value.Magnitude, vector2.Value.Value, vector2.Value.Magnitude),
            VectorDistanceMetrics.Euclidean => EuclideanDistance(vector1.Value.Value, vector2.Value.Value),
            VectorDistanceMetrics.DotProduct => DotProduct(vector1.Value.Value, vector2.Value.Value),
            VectorDistanceMetrics.Manhattan => ManhattanDistance(vector1.Value.Value, vector2.Value.Value),
            _ => throw new NotSupportedException($"{distanceMetric} is not supported")
        };
    }

    /// <summary>
    /// Calculates the cosine distance between two vectors using precomputed magnitudes.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="magnitude1">The precomputed magnitude of the first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="magnitude2">The precomputed magnitude of the second vector.</param>
    /// <returns>The cosine distance (1 - cosine similarity).</returns>
    public static double CosineDistance(double[] vector1, double magnitude1, double[] vector2, double magnitude2)
    {
        if (magnitude1 == 0 || magnitude2 == 0)
            return 1.0;

        var dot = DotProduct(vector1, vector2);
        return 1.0 - dot / (magnitude1 * magnitude2);
    }

    /// <summary>
    /// Calculates the Euclidean distance (L2 norm) between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The Euclidean distance.</returns>
    public static double EuclideanDistance(double[] v1, double[] v2)
    {
        var sum = 0.0;
        for (var i = 0; i < v1.Length; i++)
        {
            var diff = v1[i] - v2[i];
            sum += diff * diff;
        }
        return global::System.Math.Sqrt(sum);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product.</returns>
    public static double DotProduct(double[] v1, double[] v2)
    {
        var result = 0.0;
        for (var i = 0; i < v1.Length; i++)
        {
            result += v1[i] * v2[i];
        }
        return result;
    }

    /// <summary>
    /// Calculates the Manhattan distance (L1 norm) between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The Manhattan distance.</returns>
    public static double ManhattanDistance(double[] v1, double[] v2)
    {
        var distance = 0.0;
        for (var i = 0; i < v1.Length; i++)
        {
            distance += global::System.Math.Abs(v1[i] - v2[i]);
        }
        return distance;
    }
}
