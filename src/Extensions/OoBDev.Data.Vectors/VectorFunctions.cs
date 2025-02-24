using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

public static class VectorFunctions
{
    [SqlFunction(Name = "VECTOR_ELEMENT", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Element(SqlVector vector, SqlInt32 index) =>
        (vector.IsNull || index.IsNull) ? SqlDouble.Null : (SqlDouble)vector.Values[index.Value];

    [SqlFunction(Name = "VECTOR_MAGNITUDE", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Magnitude(SqlVector vector) =>
        vector.IsNull ? SqlDouble.Null : (SqlDouble)Math.Sqrt(DotProduct(vector.Values, vector.Values));

    public static double Magnitude(IReadOnlyList<double> values) =>
        Math.Sqrt(DotProduct(values, values));

    [SqlFunction(Name = "VECTOR_LENGTH", IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 VectorLength(SqlVector vector) =>
        vector.IsNull ? SqlInt32.Null : (SqlInt32)vector.Values.Count;

    [SqlFunction(Name = "VECTOR_DISTANCE", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Distance(SqlString distanceMetric, SqlVector vector1, SqlVector vector2)
    {
        if (distanceMetric.IsNull || string.IsNullOrWhiteSpace(distanceMetric.Value) ||
            vector1.IsNull ||
            vector2.IsNull)
        {
            return SqlDouble.Null;
        }
        else if (vector1.Values.Count != vector2.Values.Count)
        {
            throw new ArgumentException("Vectors must be of the same length.");
        }

        return distanceMetric.Value.ToLower() switch
        {
            VectorDistanceTypes.CosineDistance => (SqlDouble)CosineDistance(vector1.Values, vector1.Magnitude, vector2.Values, vector2.Magnitude),
            VectorDistanceTypes.CosineSimilarity => (SqlDouble)CosineSimilarity(vector1.Values, vector1.Magnitude, vector2.Values, vector2.Magnitude),
            VectorDistanceTypes.EuclideanDistance => (SqlDouble)EuclideanDistance(vector1.Values, vector2.Values),
            VectorDistanceTypes.DotProduct => (SqlDouble)DotProduct(vector1.Values, vector2.Values),
            VectorDistanceTypes.ManhattanDistance => (SqlDouble)ManhattanDistance(vector1.Values, vector2.Values),
            _ => throw new ArgumentException($"Unsupported distance metric: {distanceMetric}"),
        };
    }

    [SqlFunction(Name = "VECTOR_MIDPOINT", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector Midpoint(SqlVector vector1, SqlVector vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlVector.Null;
        }
        else if (vector1.Values.Count != vector2.Values.Count)
        {
            throw new ArgumentException("Vectors must be of the same length.");
        }

        var midpoint = new double[vector1.Values.Count];
        for (var i = 0; i < vector1.Values.Count; i++)
        {
            midpoint[i] = (vector1.Values[i] + vector2.Values[i]) / 2.0;
        }

        var vectorM = new SqlVector(midpoint);
        return vectorM;
    }

    [SqlFunction(Name = "VECTOR_ANGLE", IsDeterministic = true, IsPrecise = true)]
    internal static SqlDouble Angle(SqlVector vector1, SqlVector vector2) =>
        vector1.IsNull || vector2.IsNull ? SqlDouble.Null :
        Math.Acos(Math.Min(1, Math.Max(0, Math.Sqrt(DotProduct(vector1.Values, vector2.Values)) / (vector1.Magnitude * vector2.Magnitude))));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CosineDistance(IReadOnlyList<double> vector1, double magnitude1, IReadOnlyList<double> vector2, double magnitude2)
    {
        if (magnitude1 == 0 || magnitude2 == 0)
            return 1.0;
        return 1.0 - CosineSimilarity(vector1, magnitude1, vector2, magnitude2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CosineSimilarity(IReadOnlyList<double> vector1, double magnitude1, IReadOnlyList<double> vector2, double magnitude2)
    {
        var dot = DotProduct(vector1, vector2);
        return Math.Max(-1.0, Math.Min(1.0, dot / (magnitude1 * magnitude2)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double EuclideanDistance(IReadOnlyList<double> v1, IReadOnlyList<double> v2)
    {
        var sum = 0.0;
        for (var i = 0; i < v1.Count; i++)
        {
            var diff = v1[i] - v2[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double DotProduct(IReadOnlyList<double> v1, IReadOnlyList<double> v2)
    {
        var result = 0.0;
        for (var i = 0; i < v1.Count; i++)
        {
            result += v1[i] * v2[i];
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ManhattanDistance(IReadOnlyList<double> v1, IReadOnlyList<double> v2)
    {
        var distance = 0.0;
        for (var i = 0; i < v1.Count; i++)
        {
            distance += Math.Abs(v1[i] - v2[i]);
        }
        return distance;
    }
}
