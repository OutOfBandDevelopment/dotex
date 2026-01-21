using System.Collections;
using System.Collections.Generic;

namespace OoBDev.System.Math;

public class VectorComparer : IComparer<Vector>, IComparer<double[]>, IComparer
{
    public VectorDistanceMetrics DistanceMetric { get; init; } = VectorDistanceMetrics.Cosine;

    public int Compare(Vector x, Vector y) => Compare((object?)x, (object?)y);
    public int Compare(double[]? x, double[]? y) => Compare((object?)x, (object?)y);
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
