﻿using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OoBDev.Data.Vectors;

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

    public readonly bool IsNull => _isNull;
    public readonly IReadOnlyList<double> Values => _values;

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

    public SqlVectorF(IReadOnlyList<float> values) : this([.. values.Select(Convert.ToDouble)]) { }

    public SqlVectorF(IReadOnlyList<double> values)
    {
        _isNull = false;
        _values = values;
        _magnitude = _magnitude = VectorFunctions.MagnitudeInternal(_values);
    }

    public static SqlVectorF Null => new(true);

    [SqlMethod(
        Name = nameof(Element),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Element(SqlInt32 position) =>
        (position.IsNull || IsNull) ? SqlSingle.Null : (SqlSingle)Values[position.Value];

    [SqlMethod(
        Name = nameof(Distance),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Distance(SqlVectorF vector, SqlString metric) =>
        (SqlSingle)VectorFunctions.DistanceF(metric, this, vector);

    [SqlMethod(
        Name = nameof(Angle),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Angle(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.AngleF(this, vector);

    [SqlMethod(
        Name = nameof(Cosine),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Cosine(SqlVectorF vector) =>
       (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.CosineDistance, this, vector);

    [SqlMethod(
        Name = nameof(Similarity),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Similarity(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.CosineSimilarity, this, vector);

    [SqlMethod(
        Name = nameof(DotProduct),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle DotProduct(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.DotProduct, this, vector);

    [SqlMethod(
        Name = nameof(Euclidean),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Euclidean(SqlVectorF vector) =>
         (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.EuclideanDistance, this, vector);

    [SqlMethod(
        Name = nameof(Manhattan),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Manhattan(SqlVectorF vector) =>
        (SqlSingle)VectorFunctions.DistanceF(VectorDistanceTypes.ManhattanDistance, this, vector);

    [SqlMethod(
        Name = nameof(Midpoint),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlVectorF Midpoint(SqlVectorF vector) =>
        VectorFunctions.MidpointF(this, vector);

    [SqlMethod(
        Name = nameof(Length),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlInt32 Length() => Values.Count;


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

    public static SqlVectorF From(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return Null;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);
        var vector = new SqlVectorF();
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

    public static explicit operator SqlVectorF(SqlVector vector) => new(values: vector.Values);
    public static explicit operator SqlVectorF(float[] vector) => new(values: vector);
    public static explicit operator SqlVectorF(double[] vector) => new(values: vector);
    public static explicit operator float[](SqlVectorF vector) => [.. vector.Values.Select(Convert.ToSingle)];
    public static explicit operator double[](SqlVectorF vector) => [.. vector.Values];

    public override readonly bool Equals(object other) =>
        other is SqlVectorF matrix && Equals(matrix);

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
}
