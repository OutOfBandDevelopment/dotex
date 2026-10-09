



using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

/// <summary>
/// Provides SQL CLR functions for vector operations including distance metrics, magnitude calculations, and vector generation.
/// </summary>
public static class VectorFunctions
{
    /// <summary>
    /// Gets the value at a specific index in a double-precision vector.
    /// </summary>
    /// <param name="vector">The vector to query.</param>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>The value at the specified index, or null if the vector or index is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(Element)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Element(SqlVector vector, SqlInt32 index) =>
        (vector.IsNull || index.IsNull) ? SqlDouble.Null : (SqlDouble)vector.Values[index.Value];

    /// <summary>
    /// Gets the value at a specific index in a single-precision vector.
    /// </summary>
    /// <param name="vector">The vector to query.</param>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>The value at the specified index, or null if the vector or index is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(ElementF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle ElementF(SqlVectorF vector, SqlInt32 index) =>
        (vector.IsNull || index.IsNull) ? SqlSingle.Null : (SqlSingle)vector.Values[index.Value];

    /// <summary>
    /// Calculates the magnitude (Euclidean norm) of a double-precision vector.
    /// </summary>
    /// <param name="vector">The vector to measure.</param>
    /// <returns>The magnitude of the vector, or null if the vector is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(Magnitude)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Magnitude(SqlVector vector) =>
        vector.IsNull ? SqlDouble.Null : (SqlDouble)Math.Sqrt(DotProduct(vector.Values, vector.Values));

    /// <summary>
    /// Calculates the magnitude (Euclidean norm) of a single-precision vector.
    /// </summary>
    /// <param name="vector">The vector to measure.</param>
    /// <returns>The magnitude of the vector, or null if the vector is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(MagnitudeF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle MagnitudeF(SqlVectorF vector) =>
        vector.IsNull ? SqlSingle.Null : (SqlSingle)Math.Sqrt(DotProduct(vector.Values, vector.Values));

    /// <summary>
    /// Gets the number of elements in a double-precision vector.
    /// </summary>
    /// <param name="vector">The vector to measure.</param>
    /// <returns>The number of elements, or null if the vector is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(Length)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 Length(SqlVector vector) =>
        vector.IsNull ? SqlInt32.Null : (SqlInt32)vector.Values.Count;

    /// <summary>
    /// Gets the number of elements in a single-precision vector.
    /// </summary>
    /// <param name="vector">The vector to measure.</param>
    /// <returns>The number of elements, or null if the vector is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(LengthF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 LengthF(SqlVectorF vector) =>
        vector.IsNull ? SqlInt32.Null : (SqlInt32)vector.Values.Count;

    /// <summary>
    /// Calculates the distance or similarity between two double-precision vectors using the specified metric.
    /// </summary>
    /// <param name="distanceMetric">The metric to use (cosine, similarity, euclidean, dot, manhattan; matched case-insensitively, anything else returns NULL).</param>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The calculated distance or similarity value, or null if any parameter is null.</returns>
    /// <remarks>Returns NULL (never throws) for NULL input, vectors of different lengths or an unsupported metric. An unsupported metric name also returns NULL, so one bad row cannot fail a whole batch.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(Distance)}]", IsDeterministic = true, IsPrecise = true)]
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
            return SqlDouble.Null;
        }

        return distanceMetric.Value.ToLower() switch
        {
            VectorDistanceTypes.CosineDistance => (SqlDouble)CosineDistance(vector1.Values, vector1.Magnitude().Value, vector2.Values, vector2.Magnitude().Value),
            VectorDistanceTypes.CosineSimilarity => (SqlDouble)CosineSimilarity(vector1.Values, vector1.Magnitude().Value, vector2.Values, vector2.Magnitude().Value),
            VectorDistanceTypes.EuclideanDistance => (SqlDouble)EuclideanDistance(vector1.Values, vector2.Values),
            VectorDistanceTypes.DotProduct => (SqlDouble)DotProduct(vector1.Values, vector2.Values),
            VectorDistanceTypes.ManhattanDistance => (SqlDouble)ManhattanDistance(vector1.Values, vector2.Values),
            _ => SqlDouble.Null,
        };
    }

    /// <summary>
    /// Calculates the distance or similarity between two single-precision vectors using the specified metric.
    /// </summary>
    /// <param name="distanceMetric">The metric to use (cosine, similarity, euclidean, dot, manhattan; matched case-insensitively, anything else returns NULL).</param>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The calculated distance or similarity value, or null if any parameter is null.</returns>
    /// <remarks>Returns NULL (never throws) for NULL input, vectors of different lengths or an unsupported metric. An unsupported metric name also returns NULL, so one bad row cannot fail a whole batch.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(DistanceF)}]", IsDeterministic = true, IsPrecise = true)]
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
            return SqlSingle.Null;
        }

        return distanceMetric.Value.ToLower() switch
        {
            VectorDistanceTypes.CosineDistance => (SqlSingle)CosineDistance(vector1.Values, vector1.Magnitude().Value, vector2.Values, vector2.Magnitude().Value),
            VectorDistanceTypes.CosineSimilarity => (SqlSingle)CosineSimilarity(vector1.Values, vector1.Magnitude().Value, vector2.Values, vector2.Magnitude().Value),
            VectorDistanceTypes.EuclideanDistance => (SqlSingle)EuclideanDistance(vector1.Values, vector2.Values),
            VectorDistanceTypes.DotProduct => (SqlSingle)DotProduct(vector1.Values, vector2.Values),
            VectorDistanceTypes.ManhattanDistance => (SqlSingle)ManhattanDistance(vector1.Values, vector2.Values),
            _ => SqlSingle.Null,
        };
    }

    /// <summary>
    /// Calculates the midpoint between two double-precision vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>A vector representing the midpoint, or null if either vector is null.</returns>
    /// <remarks>Returns NULL (never throws) when vectors have different lengths.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(Midpoint)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector Midpoint(SqlVector vector1, SqlVector vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlVector.Null;
        }
        else if (vector1.Values.Count != vector2.Values.Count)
        {
            return SqlVector.Null;
        }

        var midpoint = new double[vector1.Values.Count];
        for (var i = 0; i < vector1.Values.Count; i++)
        {
            midpoint[i] = (vector1.Values[i] + vector2.Values[i]) / 2.0;
        }

        var vectorM = new SqlVector(midpoint);
        return vectorM;
    }

    /// <summary>
    /// Calculates the midpoint between two single-precision vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>A vector representing the midpoint, or null if either vector is null.</returns>
    /// <remarks>Returns NULL (never throws) when vectors have different lengths.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(MidpointF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF MidpointF(SqlVectorF vector1, SqlVectorF vector2)
    {
        if (vector1.IsNull || vector2.IsNull)
        {
            return SqlVectorF.Null;
        }
        else if (vector1.Values.Count != vector2.Values.Count)
        {
            return SqlVectorF.Null;
        }

        var midpoint = new double[vector1.Values.Count];
        for (var i = 0; i < vector1.Values.Count; i++)
        {
            midpoint[i] = (vector1.Values[i] + vector2.Values[i]) / 2.0;
        }

        var vectorM = new SqlVectorF(midpoint);
        return vectorM;
    }

    /// <summary>
    /// Calculates the angle in radians between two double-precision vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The angle in radians (0 to pi), or NULL when undefined.</returns>
    /// <remarks>
    /// Returns NULL, never throws, when the angle is undefined: either input is NULL, the vectors have different
    /// lengths, or either vector has zero magnitude. NULL is not a match score. Callers that order by angle must
    /// filter these rows out (<c>WHERE angle IS NOT NULL</c>) or sort them last, because SQL Server sorts NULL first
    /// in an ascending <c>ORDER BY</c>.
    /// </remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(Angle)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlDouble Angle(SqlVector vector1, SqlVector vector2) =>
        vector1.IsNull || vector2.IsNull ? SqlDouble.Null :
        AngleInternal(vector1.Values, vector2.Values) is { } angle ? (SqlDouble)angle : SqlDouble.Null;

    /// <summary>
    /// Calculates the angle in radians between two single-precision vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The angle in radians (0 to pi), or NULL when undefined.</returns>
    /// <remarks>
    /// Returns NULL, never throws, when the angle is undefined: either input is NULL, the vectors have different
    /// lengths, or either vector has zero magnitude. NULL is not a match score. Callers that order by angle must
    /// filter these rows out (<c>WHERE angle IS NOT NULL</c>) or sort them last, because SQL Server sorts NULL first
    /// in an ascending <c>ORDER BY</c>.
    /// </remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(AngleF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlSingle AngleF(SqlVectorF vector1, SqlVectorF vector2) =>
        vector1.IsNull || vector2.IsNull ? SqlSingle.Null :
        AngleInternal(vector1.Values, vector2.Values) is { } angle ? (SqlSingle)angle : SqlSingle.Null;

    /// <summary>
    /// Generates a random double-precision vector with values between 0 and 1.
    /// </summary>
    /// <param name="length">The number of elements in the vector.</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with random values, or null if length is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(Random)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector Random(SqlInt32 length, SqlInt32 seed)
    {
        if (length.IsNull) return SqlVector.Null;

        var realLength = length.Value;
        //xor seed with length multiplied by prime to make different length vectors have different values
        var realSeed = (seed.IsNull ? Environment.TickCount : seed.Value) ^ (realLength * 1309);
        var rand = new Random(realSeed);

        var vector = new double[realLength];

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = rand.NextDouble();
        }

        return new SqlVector(vector);
    }

    /// <summary>
    /// Generates a random single-precision vector with values between 0 and 1.
    /// </summary>
    /// <param name="length">The number of elements in the vector.</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with random values, or null if length is null.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(RandomF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF RandomF(SqlInt32 length, SqlInt32 seed) =>
        new(Random(length, seed).Values);

    /// <summary>
    /// Generates a random double-precision vector with uniform distribution in the specified range.
    /// </summary>
    /// <param name="length">The number of elements in the vector.</param>
    /// <param name="min">The minimum value (defaults to -1.0).</param>
    /// <param name="max">The maximum value (defaults to 1.0).</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with uniformly distributed random values.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(Uniform)}]", IsDeterministic = true, IsPrecise = true)]
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

    /// <summary>
    /// Generates a random double-precision vector with element-wise uniform distribution using vector bounds.
    /// </summary>
    /// <param name="min">The minimum values for each element.</param>
    /// <param name="max">The maximum values for each element.</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with uniformly distributed random values.</returns>
    /// <remarks>Returns NULL (never throws) when min and max have different lengths.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(UniformV)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVector UniformV(SqlVector min, SqlVector max, SqlInt32 seed)
    {
        if (min.IsNull || max.IsNull) return SqlVector.Null;
        if (min.Length() != max.Length()) return SqlVector.Null;

        var random = Random(min.Length(), seed);
        if (random.IsNull) return SqlVector.Null;

        var realMin = min.Values;
        var realMax = max.Values;

        var vector = random.Values.ToArray();
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] * (realMax[i] - realMin[i]) + realMin[i];
        }

        return new SqlVector(vector);
    }

    /// <summary>
    /// Generates a random single-precision vector with uniform distribution in the specified range.
    /// </summary>
    /// <param name="length">The number of elements in the vector.</param>
    /// <param name="min">The minimum value (defaults to -1.0).</param>
    /// <param name="max">The maximum value (defaults to 1.0).</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with uniformly distributed random values.</returns>
    [SqlFunction(Name = $"[embedding].[{nameof(UniformF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF UniformF(SqlInt32 length, SqlDouble min, SqlDouble max, SqlInt32 seed) =>
        new(Uniform(length, min, max, seed).Values);


    /// <summary>
    /// Generates a random single-precision vector with element-wise uniform distribution using vector bounds.
    /// </summary>
    /// <param name="min">The minimum values for each element.</param>
    /// <param name="max">The maximum values for each element.</param>
    /// <param name="seed">The random seed (null uses current time).</param>
    /// <returns>A vector with uniformly distributed random values.</returns>
    /// <remarks>Returns NULL (never throws) when min and max have different lengths.</remarks>
    [SqlFunction(Name = $"[embedding].[{nameof(UniformVF)}]", IsDeterministic = true, IsPrecise = true)]
    public static SqlVectorF UniformVF(SqlVectorF min, SqlVectorF max, SqlInt32 seed)
    {
        if (min.IsNull || max.IsNull) return SqlVectorF.Null;
        if (min.Length() != max.Length()) return SqlVectorF.Null;

        var random = Random(min.Length(), seed);
        if (random.IsNull) return SqlVectorF.Null;

        var realMin = min.Values;
        var realMax = max.Values;

        var vector = random.Values.ToArray();
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] * (realMax[i] - realMin[i]) + realMin[i];
        }

        return new SqlVectorF(vector);
    }

    /// <summary>
    /// Calculates the magnitude of a vector using its values.
    /// </summary>
    /// <param name="values">The vector values.</param>
    /// <returns>The magnitude (Euclidean norm).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double MagnitudeInternal(IReadOnlyList<double> values) =>
        Math.Sqrt(DotProduct(values, values));

    /// <summary>
    /// Calculates the angle in radians between two vectors, or null when it is undefined
    /// (different lengths, or a zero-magnitude vector).
    /// </summary>
    internal static double? AngleInternal(IReadOnlyList<double> v1, IReadOnlyList<double> v2)
    {
        if (v1.Count != v2.Count) return null;
        var magnitude1 = MagnitudeInternal(v1);
        var magnitude2 = MagnitudeInternal(v2);
        if (magnitude1 == 0 || magnitude2 == 0 || double.IsNaN(magnitude1) || double.IsNaN(magnitude2)) return null;
        var angle = Math.Acos(CosineSimilarity(v1, magnitude1, v2, magnitude2));
        return double.IsNaN(angle) ? null : angle;
    }

    /// <summary>
    /// Calculates cosine distance between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector values.</param>
    /// <param name="magnitude1">The magnitude of the first vector.</param>
    /// <param name="vector2">The second vector values.</param>
    /// <param name="magnitude2">The magnitude of the second vector.</param>
    /// <returns>The cosine distance (1 - cosine similarity).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CosineDistance(IReadOnlyList<double> vector1, double magnitude1, IReadOnlyList<double> vector2, double magnitude2) =>
        magnitude1 == 0 || magnitude2 == 0 ? 1.0 : 1.0 - CosineSimilarity(vector1, magnitude1, vector2, magnitude2);

    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector values.</param>
    /// <param name="magnitude1">The magnitude of the first vector.</param>
    /// <param name="vector2">The second vector values.</param>
    /// <param name="magnitude2">The magnitude of the second vector.</param>
    /// <returns>The cosine similarity (between -1 and 1).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CosineSimilarity(IReadOnlyList<double> vector1, double magnitude1, IReadOnlyList<double> vector2, double magnitude2)
    {
        var dot = DotProduct(vector1, vector2);
        return Math.Max(-1.0, Math.Min(1.0, dot / (magnitude1 * magnitude2)));
    }

    /// <summary>
    /// Calculates Euclidean distance between two vectors.
    /// </summary>
    /// <param name="v1">The first vector values.</param>
    /// <param name="v2">The second vector values.</param>
    /// <returns>The Euclidean distance.</returns>
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

    /// <summary>
    /// Calculates dot product of two vectors.
    /// </summary>
    /// <param name="v1">The first vector values.</param>
    /// <param name="v2">The second vector values.</param>
    /// <returns>The dot product.</returns>
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

    /// <summary>
    /// Calculates Manhattan distance (L1 norm) between two vectors.
    /// </summary>
    /// <param name="v1">The first vector values.</param>
    /// <param name="v2">The second vector values.</param>
    /// <returns>The Manhattan distance.</returns>
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
