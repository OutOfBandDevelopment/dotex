namespace OoBDev.System.Math;

/// <summary>
/// Defines the available distance metrics for comparing vectors.
/// </summary>
public enum VectorDistanceMetrics
{
    /// <summary>
    /// Cosine distance (1 - cosine similarity), measuring the angle between vectors.
    /// </summary>
    Cosine,

    /// <summary>
    /// Euclidean distance (L2 norm), measuring the straight-line distance between vectors.
    /// </summary>
    Euclidean,

    /// <summary>
    /// Dot product, measuring the projection of one vector onto another.
    /// </summary>
    DotProduct,

    /// <summary>
    /// Manhattan distance (L1 norm), measuring the sum of absolute differences.
    /// </summary>
    Manhattan
}
