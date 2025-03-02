using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

[TypeConverter(typeof(SqlVectorConverter))]
[Serializable]
[SqlUserDefinedType(
    Format.UserDefined,
    Name = "[embedding].[Vector]",
    IsByteOrdered = true,
    MaxByteSize = -1)]
public record struct SqlVector : INullable, IBinarySerialize
{
    private const int Version = 0x01;

    private readonly bool _isNull;
    private IReadOnlyList<double> _values;
    private double _magnitude;

    public readonly bool IsNull => _isNull;

    public readonly IReadOnlyList<double> Values => _values;

    [SqlMethod(
        Name = nameof(Magnitude),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly double Magnitude() => _magnitude;

    private SqlVector(bool isNull)
    {
        if (!isNull) throw new InvalidOperationException();
        _isNull = isNull;
        _values = Array.Empty<double>();
        _magnitude = 0.0;
    }

    public SqlVector(IReadOnlyList<float> values) : this([.. values.Select(Convert.ToDouble)]) { }

    public SqlVector(IReadOnlyList<double> values)
    {
        _isNull = false;
        _values = values;
        _magnitude = _magnitude = VectorFunctions.MagnitudeInternal(values);
    }

    internal static SqlVector Null => new(true);

    [SqlMethod(
        Name = nameof(Element),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Element(SqlInt32 position) =>
        (position.IsNull || IsNull) ? SqlDouble.Null : (SqlDouble)Values[position.Value];

    [SqlMethod(
        Name = nameof(Distance),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Distance(SqlVector vector, SqlString metric) =>
        VectorFunctions.Distance(metric, this, vector);

    [SqlMethod(
        Name = nameof(Angle),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Angle(SqlVector vector) =>
        VectorFunctions.Angle(this, vector);

    [SqlMethod(
        Name = nameof(Cosine),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Cosine(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.CosineDistance, this, vector);

    [SqlMethod(
        Name = nameof(Similarity),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Similarity(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.CosineSimilarity, this, vector);

    [SqlMethod(
        Name = nameof(DotProduct),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble DotProduct(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.DotProduct, this, vector);

    [SqlMethod(
        Name = nameof(Euclidean),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Euclidean(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.EuclideanDistance, this, vector);

    [SqlMethod(
        Name = nameof(Manhattan),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlDouble Manhattan(SqlVector vector) =>
        VectorFunctions.Distance(VectorDistanceTypes.ManhattanDistance, this, vector);

    [SqlMethod(
        Name = nameof(Midpoint),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlVector Midpoint(SqlVector vector) =>
        VectorFunctions.Midpoint(this, vector);

    [SqlMethod(
        Name = nameof(Length),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlInt32 Length() => Values.Count;

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
        if (version > 0 || reader.BaseStream.Position < reader.BaseStream.Length)
        {
            _magnitude = reader.ReadDouble();
        }
        else
        {
            _magnitude = VectorFunctions.Magnitude(this).Value;
        }
    }

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

    public static SqlVector From(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return Null;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);
        var vector = new SqlVector();
        vector.Read(reader);
        return vector;
    }

    public readonly byte[] ToBytes()
    {
        if (IsNull) return [];
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Write(writer);
        return stream.ToArray();
    }

    public override readonly string ToString()
    {
        if (_values == null || _values.Count == 0)
            return "[]";

        var formattedValues = _values
            .Select(v => v.ToString("e7", CultureInfo.InvariantCulture)) // Always use '.' as separator
            .ToArray();

        return "[" + string.Join(",", formattedValues) + "]";
    }

    public static implicit operator SqlVector(SqlVectorF vector) => new(values: vector.Values);
    public static implicit operator SqlVector(float[] vector) => new(values: vector);
    public static implicit operator float[](SqlVector vector) => [.. vector.Values.Select(Convert.ToSingle)];
    public static implicit operator double[](SqlVector vector) => [.. vector.Values];
}
