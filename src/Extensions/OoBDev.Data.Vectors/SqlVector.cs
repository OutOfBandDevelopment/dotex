using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR user-defined type representing a double-precision floating-point vector for embedding and vector operations.
/// </summary>
[Serializable]
[SqlUserDefinedType(
    Format.UserDefined,
    Name = "[embedding].[Vector]",
    IsByteOrdered = true,
    MaxByteSize = -1)]
public struct SqlVector : INullable, IBinarySerialize, IEquatable<SqlVector>
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
    /// <returns>The magnitude of the vector.</returns>
    [SqlMethod(
        Name = nameof(Magnitude),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlDouble Magnitude() => _magnitude;

    private SqlVector(bool isNull)
    {
        if (!isNull) throw new InvalidOperationException();
        _isNull = isNull;
        _values = Array.Empty<double>();
        _magnitude = 0.0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlVector"/> struct from single-precision values.
    /// </summary>
    /// <param name="values">The vector values as floats.</param>
    public SqlVector(IReadOnlyList<float> values) : this([.. values.Select(Convert.ToDouble)]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlVector"/> struct from double-precision values.
    /// </summary>
    /// <param name="values">The vector values as doubles.</param>
    public SqlVector(IReadOnlyList<double> values)
    {
        _isNull = false;
        _values = values;
        _magnitude = _magnitude = VectorFunctions.MagnitudeInternal(values);
    }

    /// <summary>
    /// Gets a null vector instance.
    /// </summary>
    public static SqlVector Null => new(true);

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
    public SqlDouble Element(SqlInt32 position) =>
        (position.IsNull || IsNull) ? SqlDouble.Null : (SqlDouble)Values[position.Value];

    /// <summary>
    /// Calculates the distance to another vector using the specified metric.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <param name="metric">The distance metric to use.</param>
    /// <returns>The calculated distance or similarity value.</returns>
    [SqlMethod(
        Name = nameof(Distance),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Distance(SqlVector vector, SqlString metric) =>
        VectorFunctions.Distance(metric, this, vector);

    /// <summary>
    /// Calculates the angle between this vector and another vector.
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
    public SqlDouble Angle(SqlVector vector) =>
        VectorFunctions.Angle(this, vector);

    /// <summary>
    /// Calculates the cosine distance to another vector.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The cosine distance (1 - cosine similarity).</returns>
    [SqlMethod(
        Name = nameof(Cosine),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Cosine(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.CosineDistance, this, vector);

    /// <summary>
    /// Calculates the cosine similarity to another vector.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The cosine similarity (between -1 and 1).</returns>
    [SqlMethod(
        Name = nameof(Similarity),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Similarity(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.CosineSimilarity, this, vector);

    /// <summary>
    /// Calculates the dot product with another vector.
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
    public SqlDouble DotProduct(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.DotProduct, this, vector);

    /// <summary>
    /// Calculates the Euclidean distance to another vector.
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
    public SqlDouble Euclidean(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.EuclideanDistance, this, vector);

    /// <summary>
    /// Calculates the Manhattan distance to another vector.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The Manhattan distance (L1 norm).</returns>
    [SqlMethod(
        Name = nameof(Manhattan),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Manhattan(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.ManhattanDistance, this, vector);

    /// <summary>
    /// Calculates the midpoint between this vector and another vector.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>A vector representing the midpoint.</returns>
    [SqlMethod(
        Name = nameof(Midpoint),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlVector Midpoint(SqlVector vector) =>
        VectorFunctions.Midpoint(this, vector);

    /// <summary>
    /// Gets the number of elements in the vector.
    /// </summary>
    /// <returns>The number of elements.</returns>
    [SqlMethod(
        Name = nameof(Length),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlInt32 Length() => Values.Count;

    /// <summary>
    /// Deserializes the vector from a binary stream.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    public void Read(BinaryReader reader)
    {
        var header = reader.ReadInt32();
        var version = (header & 0xff000000) >> 24;
        var length = header & 0x00ffffff;

        var values = new double[length];
        for (var i = 0; i < length; i++)
        {
            values[i] = reader.ReadDouble();
        }
        _values = values;
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            _magnitude = reader.ReadDouble();
        }
        else
        {
            _magnitude = VectorFunctions.Magnitude(this).Value;
        }
    }

    /// <summary>
    /// Serializes the vector to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <exception cref="NotSupportedException">Thrown when the vector length exceeds the maximum supported size.</exception>
    public readonly void Write(BinaryWriter writer)
    {
        var count = _values.Count;
        if (count < 0 || count > 0x00ffffff) throw new NotSupportedException();
        var header = Version << 24 | count;

        writer.Write(header);
        foreach (var value in _values)
        {
            writer.Write(value);
        }
        writer.Write(_magnitude);
    }

    /// <summary>
    /// Parses a string representation of a vector.
    /// </summary>
    /// <param name="input">The string to parse (format: "[1.0,2.0,3.0]" or "1.0,2.0,3.0").</param>
    /// <returns>The parsed vector, or null if the input is null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when the input format is invalid.</exception>
    public static SqlVector Parse(SqlString input)
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
                .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();

            return new SqlVector(values);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid input format.");
        }
    }

    /// <summary>
    /// Creates a vector from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array containing the serialized vector.</param>
    /// <returns>The deserialized vector, or null if the bytes are null or empty.</returns>
    public static SqlVector From(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return Null;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);
        var vector = new SqlVector();
        vector.Read(reader);
        return vector;
    }

    /// <summary>
    /// Serializes the vector to a byte array.
    /// </summary>
    /// <returns>The serialized vector as a byte array, or an empty array if the vector is null.</returns>
    public readonly byte[] ToBytes()
    {
        if (IsNull) return [];
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Write(writer);
        return stream.ToArray();
    }

    /// <summary>
    /// Returns a string representation of the vector in scientific notation.
    /// </summary>
    /// <returns>A string in the format "[1.0e+000,2.0e+000]".</returns>
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
    /// Explicitly converts a single-precision vector to a double-precision vector.
    /// </summary>
    /// <param name="vector">The single-precision vector to convert.</param>
    public static explicit operator SqlVector(SqlVectorF vector) => new(values: vector.Values);

    /// <summary>
    /// Explicitly converts a float array to a vector.
    /// </summary>
    /// <param name="vector">The float array to convert.</param>
    public static explicit operator SqlVector(float[] vector) => new(values: vector);

    /// <summary>
    /// Explicitly converts a double array to a vector.
    /// </summary>
    /// <param name="vector">The double array to convert.</param>
    public static explicit operator SqlVector(double[] vector) => new(values: vector);

    /// <summary>
    /// Explicitly converts a vector to a float array.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static explicit operator float[](SqlVector vector) => [.. vector.Values.Select(Convert.ToSingle)];

    /// <summary>
    /// Explicitly converts a vector to a double array.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static explicit operator double[](SqlVector vector) => [.. vector.Values];

    /// <summary>
    /// Determines whether the specified object is equal to this vector.
    /// </summary>
    /// <param name="other">The object to compare.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override readonly bool Equals(object other) =>
        other is SqlVector matrix && Equals(matrix);

    /// <summary>
    /// Determines whether the specified vector is equal to this vector.
    /// </summary>
    /// <param name="other">The vector to compare.</param>
    /// <returns>True if all elements are equal; otherwise, false.</returns>
    public readonly bool Equals(SqlVector other)
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
    /// <returns>A hash code for the current vector.</returns>
    public override readonly int GetHashCode() => _isNull.GetHashCode() * 31 + _values.Sum(i => i.GetHashCode() * 47) + _magnitude.GetHashCode();

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>True if the vectors are equal; otherwise, false.</returns>
    public static bool operator ==(SqlVector left, SqlVector right) => left.Equals(right);

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>True if the vectors are not equal; otherwise, false.</returns>
    public static bool operator !=(SqlVector left, SqlVector right) => !(left == right);
}
