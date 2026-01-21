using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR user-defined type representing a single-precision floating-point vector for embedding and vector operations.
/// </summary>
[Serializable]
[SqlUserDefinedType(
    Format.UserDefined,
    Name = "[embedding].[VectorF]",
    IsByteOrdered = true,
    MaxByteSize = -1)]
public struct SqlVectorF : INullable, IBinarySerialize, IEquatable<SqlVectorF>
{
    private const int Version = 0x01;

    private readonly bool _isNull;
    private IReadOnlyList<double> _values;
    private double _magnitude;

    /// <summary>
    /// Gets a value indicating whether this vector is null.
    /// </summary>
    public readonly bool IsNull => _isNull;

    /// <summary>
    /// Gets the vector values as a read-only list.
    /// </summary>
    public readonly IReadOnlyList<double> Values => _values;

    /// <summary>
    /// Gets the magnitude (Euclidean norm) of this vector.
    /// </summary>
    /// <returns>The magnitude of the vector as a single-precision value.</returns>
    [SqlMethod(
        Name = nameof(Magnitude),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly SqlSingle Magnitude() => (float)_magnitude;

    private SqlVectorF(bool isNull)
    {
        if (!isNull) throw new InvalidOperationException();
        _isNull = isNull;
        _values = Array.Empty<double>();
        _magnitude = 0.0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlVectorF"/> struct from single-precision values.
    /// </summary>
    /// <param name="values">The vector values as floats.</param>
    public SqlVectorF(IReadOnlyList<float> values) : this([.. values.Select(Convert.ToDouble)]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlVectorF"/> struct from double-precision values.
    /// </summary>
    /// <param name="values">The vector values as doubles.</param>
    public SqlVectorF(IReadOnlyList<double> values)
    {
        _isNull = false;
        _values = values;
        _magnitude = _magnitude = VectorFunctions.MagnitudeInternal(_values);
    }

    /// <summary>
    /// Gets a null vector instance.
    /// </summary>
    public static SqlVectorF Null => new(true);

    /// <summary>
    /// Gets the value at the specified position in the vector.
    /// </summary>
    /// <param name="position">The zero-based index of the element.</param>
    /// <returns>The value at the specified position, or null if the position or vector is null.</returns>
    [SqlMethod(
        Name = nameof(Element),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Element(SqlInt32 position) =>
        (position.IsNull || IsNull) ? SqlSingle.Null : (SqlSingle)Values[position.Value];

    /// <summary>
    /// Calculates the distance between this vector and another using the specified metric.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <param name="metric">The distance metric (cosine, similarity, euclidean, dot, manhattan).</param>
    /// <returns>The calculated distance.</returns>
    [SqlMethod(
        Name = nameof(Distance),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Distance(SqlVectorF vector, SqlString metric) =>
        (SqlSingle)VectorFunctions.DistanceF(metric, this, vector);

    /// <summary>
    /// Calculates the angle in radians between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The angle in radians.</returns>
    [SqlMethod(
        Name = nameof(Angle),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Angle(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.AngleF(this, vector);

    /// <summary>
    /// Calculates the cosine distance between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The cosine distance.</returns>
    [SqlMethod(
        Name = nameof(Cosine),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Cosine(SqlVectorF vector) =>
       (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.CosineDistance, this, vector);

    /// <summary>
    /// Calculates the cosine similarity between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The cosine similarity.</returns>
    [SqlMethod(
        Name = nameof(Similarity),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Similarity(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.CosineSimilarity, this, vector);

    /// <summary>
    /// Calculates the dot product of this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The dot product.</returns>
    [SqlMethod(
        Name = nameof(DotProduct),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle DotProduct(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.DotProduct, this, vector);

    /// <summary>
    /// Calculates the Euclidean distance between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The Euclidean distance.</returns>
    [SqlMethod(
        Name = nameof(Euclidean),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Euclidean(SqlVectorF vector) =>
         (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.EuclideanDistance, this, vector);

    /// <summary>
    /// Calculates the Manhattan distance between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The Manhattan distance.</returns>
    [SqlMethod(
        Name = nameof(Manhattan),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Manhattan(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.ManhattanDistance, this, vector);

    /// <summary>
    /// Calculates the midpoint between this vector and another.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>A new vector representing the midpoint.</returns>
    [SqlMethod(
        Name = nameof(Midpoint),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlVectorF Midpoint(SqlVectorF vector) =>
        VectorFunctions.MidpointF(this, vector);

    /// <summary>
    /// Gets the number of elements in this vector.
    /// </summary>
    /// <returns>The vector length.</returns>
    [SqlMethod(
        Name = nameof(Length),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlInt32 Length() => Values.Count;


    /// <summary>
    /// Scales this vector by a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar multiplier.</param>
    /// <returns>A new scaled vector.</returns>
    [SqlMethod(
        Name = nameof(Scale),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlVectorF Scale(SqlSingle scalar)
    {
        if (scalar.IsNull) return Null;

        var data = new double[Values.Count];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = Values[i] * scalar.Value;
        }

        return new(data);
    }


    /// <summary>
    /// Deserializes this vector from a binary reader.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    public void Read(BinaryReader reader)
    {
        var header = reader.ReadInt32();
        var version = (header & 0xff000000) >> 24;
        var length = header & 0x00ffffff;

        var values = new double[length];
        for (var i = 0; i < length; i++)
        {
            values[i] = reader.ReadSingle();
        }
        _values = values;
        _magnitude = reader.ReadSingle();
    }

    /// <summary>
    /// Serializes this vector to a binary writer.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    public readonly void Write(BinaryWriter writer)
    {
        var count = _values.Count;
        if (count < 0 || count > 0x00ffffff) throw new NotSupportedException();
        var header = Version << 24 | count;

        writer.Write(header);
        foreach (var value in _values)
        {
            writer.Write((float)value);
        }
        writer.Write((float)_magnitude);
    }

    /// <summary>
    /// Parses a string representation of a vector.
    /// </summary>
    /// <param name="input">The input string in format "[1.0,2.0,3.0]".</param>
    /// <returns>The parsed vector.</returns>
    public static SqlVectorF Parse(SqlString input)
    {
        if (input.IsNull || string.IsNullOrWhiteSpace(input.Value))
            return Null;

        var inputValue = input.Value.Trim();
        try
        {
            if (inputValue.StartsWith("[") && inputValue.EndsWith("]"))
            {
                inputValue = inputValue.Substring(1, inputValue.Length - 2);
            }

            var values = inputValue
                .Split([','], StringSplitOptions.RemoveEmptyEntries)
                .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();

            return new SqlVectorF(values);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid input format.");
        }
    }

    /// <summary>
    /// Creates a vector from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array.</param>
    /// <returns>The deserialized vector.</returns>
    public static SqlVectorF From(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return Null;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);
        var vector = new SqlVectorF();
        vector.Read(reader);
        return vector;
    }

    /// <summary>
    /// Converts this vector to a byte array.
    /// </summary>
    /// <returns>The serialized byte array.</returns>
    public readonly byte[] ToBytes()
    {
        if (IsNull) return [];
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Write(writer);
        return stream.ToArray();
    }

    /// <summary>
    /// Returns a string representation of this vector.
    /// </summary>
    /// <returns>A string in format "[1.0,2.0,3.0]".</returns>
    public override readonly string ToString()
    {
        if (_values == null || _values.Count == 0)
            return "[]";

        var formattedValues = _values
            .Select(v => v.ToString("e7", CultureInfo.InvariantCulture)) // Always use '.' as separator
            .ToArray();

        return "[" + string.Join(",", formattedValues) + "]";
    }

    /// <summary>
    /// Converts a SqlVector to a SqlVectorF.
    /// </summary>
    public static explicit operator SqlVectorF(SqlVector vector) => new(values: vector.Values);
    /// <summary>
    /// Converts a float array to a SqlVectorF.
    /// </summary>
    public static explicit operator SqlVectorF(float[] vector) => new(values: vector);
    /// <summary>
    /// Converts a double array to a SqlVectorF.
    /// </summary>
    public static explicit operator SqlVectorF(double[] vector) => new(values: vector);
    /// <summary>
    /// Converts a SqlVectorF to a float array.
    /// </summary>
    public static explicit operator float[](SqlVectorF vector) => [.. vector.Values.Select(Convert.ToSingle)];
    /// <summary>
    /// Converts a SqlVectorF to a double array.
    /// </summary>
    public static explicit operator double[](SqlVectorF vector) => [.. vector.Values];

    /// <summary>
    /// Determines whether this vector equals another object.
    /// </summary>
    /// <param name="other">The object to compare.</param>
    /// <returns>True if equal; otherwise, false.</returns>
    public override readonly bool Equals(object other) =>
        other is SqlVectorF matrix && Equals(matrix);

    /// <summary>
    /// Determines whether this vector equals another SqlVectorF.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>True if equal; otherwise, false.</returns>
    public readonly bool Equals(SqlVectorF other)
    {
        if (IsNull != other.IsNull) return false;

        if (Values.Count != other.Values.Count) return false;

        for (var i = 0; i < Values.Count; i++)
        {
            if (_values[i] != other.Values[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for this vector.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override readonly int GetHashCode() => _isNull.GetHashCode() * 31 + _values.Sum(i => i.GetHashCode() * 47) + _magnitude.GetHashCode();
    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(SqlVectorF left, SqlVectorF right) => left.Equals(right);
    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(SqlVectorF left, SqlVectorF right) => !(left == right);
}
