using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

public static class VectorFunctions
{
    [SqlFunction(Name = $"Vector.{nameof(Element)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Element(SqlVector vector, SqlInt32 index) =>
        (vector.IsNull || index.IsNull) ? SqlDouble.Null : (SqlDouble)vector.Values[index.Value];

    [SqlFunction(Name = $"Vector.{nameof(ElementF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle ElementF(SqlVectorF vector, SqlInt32 index) =>
        (vector.IsNull || index.IsNull) ? SqlSingle.Null : (SqlSingle)vector.Values[index.Value];

    [SqlFunction(Name = $"Vector.{nameof(Magnitude)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Magnitude(SqlVector vector) =>
        vector.IsNull ? SqlDouble.Null : (SqlDouble)Math.Sqrt(DotProduct(vector.Values, vector.Values));

    [SqlFunction(Name = $"Vector.{nameof(MagnitudeF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle MagnitudeF(SqlVectorF vector) =>
        vector.IsNull ? SqlSingle.Null : (SqlSingle)Math.Sqrt(DotProduct(vector.Values, vector.Values));

    [SqlFunction(Name = $"Vector.{nameof(Length)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 Length(SqlVector vector) =>
        vector.IsNull ? SqlInt32.Null : (SqlInt32)vector.Values.Count;

    [SqlFunction(Name = $"Vector.{nameof(LengthF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 LengthF(SqlVectorF vector) =>
        vector.IsNull ? SqlInt32.Null : (SqlInt32)vector.Values.Count;

    [SqlFunction(Name = $"Vector.{nameof(Distance)}", IsDeterministic = true, IsPrecise = true)]
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

    [SqlFunction(Name = $"Vector.{nameof(DistanceF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle DistanceF(SqlString distanceMetric, SqlVectorF vector1, SqlVectorF vector2)
    {
        if (distanceMetric.IsNull || string.IsNullOrWhiteSpace(distanceMetric.Value) ||
            vector1.IsNull ||
            vector2.IsNull)
        {
            return SqlSingle.Null;
        }
        else if (vector1.Values.Count != vector2.Values.Count)
        {
            throw new ArgumentException("Vectors must be of the same length.");
        }

        return distanceMetric.Value.ToLower() switch
        {
            VectorDistanceTypes.CosineDistance => (SqlSingle)CosineDistance(vector1.Values, vector1.Magnitude, vector2.Values, vector2.Magnitude),
            VectorDistanceTypes.CosineSimilarity => (SqlSingle)CosineSimilarity(vector1.Values, vector1.Magnitude, vector2.Values, vector2.Magnitude),
            VectorDistanceTypes.EuclideanDistance => (SqlSingle)EuclideanDistance(vector1.Values, vector2.Values),
            VectorDistanceTypes.DotProduct => (SqlSingle)DotProduct(vector1.Values, vector2.Values),
            VectorDistanceTypes.ManhattanDistance => (SqlSingle)ManhattanDistance(vector1.Values, vector2.Values),
            _ => throw new ArgumentException($"Unsupported distance metric: {distanceMetric}"),
        };
    }

    [SqlFunction(Name = $"Vector.{nameof(Midpoint)}", IsDeterministic = true, IsPrecise = true)]
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

    [SqlFunction(Name = $"Vector.{nameof(MidpointF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF MidpointF(SqlVectorF vector1, SqlVectorF vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlVectorF.Null;
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

        var vectorM = new SqlVectorF(midpoint);
        return vectorM;
    }

    [SqlFunction(Name = $"Vector.{nameof(Angle)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Angle(SqlVector vector1, SqlVector vector2) =>
        vector1.IsNull || vector2.IsNull ? SqlDouble.Null :
        (SqlDouble)Math.Acos(
            Math.Min(1, Math.Max(0,
                Math.Sqrt(DotProduct(vector1.Values, vector2.Values)) / (vector1.Magnitude * vector2.Magnitude))
                )
            );

    [SqlFunction(Name = $"Vector.{nameof(AngleF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle AngleF(SqlVectorF vector1, SqlVectorF vector2) =>
        vector1.IsNull || vector2.IsNull ? SqlSingle.Null :
        (SqlSingle)Math.Acos(
            Math.Min(1, Math.Max(0, 
                Math.Sqrt(DotProduct(vector1.Values, vector2.Values)) / (vector1.Magnitude * vector2.Magnitude))
                )
            );

    [SqlFunction(Name = $"Vector.{nameof(Random)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector Random(SqlInt32 length, SqlInt32 seed)
    {
        if (length.IsNull) return SqlVector.Null;

        var realLength = length.Value;
        //xor seed with length multiplied by prime to make different length vectors have different values
        var realSeed = (seed.IsNull ? (int)DateTime.Now.Ticks : seed.Value) ^ (realLength * 1309);
        var rand = new Random(realSeed);

        var vector = new double[realLength];

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = rand.NextDouble();
        }

        return new SqlVector(vector);
    }

    [SqlFunction(Name = $"Vector.{nameof(RandomF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF RandomF(SqlInt32 length, SqlInt32 seed) =>
        Random(length, seed);

    [SqlFunction(Name = $"Vector.{nameof(Uniform)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector Uniform(SqlInt32 length, SqlDouble min, SqlDouble max, SqlInt32 seed)
    {
        var random = Random(length, seed);
        if (random.IsNull) return SqlVector.Null;

        var realMin = min.IsNull ? -1.0 : min.Value;
        var realMax = max.IsNull ? 1.0 : max.Value;

        var vector = random.Values.ToArray();
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] * (realMax - realMin) + realMin;
        }

        return new SqlVector(vector);
    }

    [SqlFunction(Name = $"Vector.{nameof(UniformF)}", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF UniformF(SqlInt32 length, SqlDouble min, SqlDouble max, SqlInt32 seed) =>
        Uniform(length, min, max, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double MagnitudeInternal(IReadOnlyList<double> values) =>
        Math.Sqrt(DotProduct(values, values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CosineDistance(IReadOnlyList<double> vector1, double magnitude1, IReadOnlyList<double> vector2, double magnitude2) => magnitude1 == 0 || magnitude2 == 0 ? 1.0 : 1.0 - CosineSimilarity(vector1, magnitude1, vector2, magnitude2);

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
