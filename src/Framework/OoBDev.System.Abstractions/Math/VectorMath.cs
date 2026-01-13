using System;

namespace OoBDev.Common.Math;

public class VectorMath
{
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

    public static double CosineDistance(double[] vector1, double magnitude1, double[] vector2, double magnitude2)
    {
        if (magnitude1 == 0 || magnitude2 == 0)
            return 1.0;

        var dot = DotProduct(vector1, vector2);
        return 1.0 - dot / (magnitude1 * magnitude2);
    }

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

    public static double DotProduct(double[] v1, double[] v2)
    {
        var result = 0.0;
        for (var i = 0; i < v1.Length; i++)
        {
            result += v1[i] * v2[i];
        }
        return result;
    }

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
