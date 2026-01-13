using OoBDev.System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Common.Math;

public readonly record struct Vector
{
    public Vector(IEnumerable<float> vector) : this(vector.Select(Convert.ToDouble))
    {
    }
    public Vector(IEnumerable<double> vector) : this(vector.ToArray())
    {
    }
    public Vector(double[] vector)
    {
        Value = vector;
        Magnitude = global::System.Math.Sqrt(VectorMath.DotProduct(vector, vector));
    }

    public double[] Value { get; init; }
    public double Magnitude { get; init; }

    public double? Distance(Vector? vector, VectorDistanceMetrics distanceMetric = VectorDistanceMetrics.Cosine) =>
        VectorMath.Distance(this, vector, distanceMetric);

    public static Vector Create(IEnumerable<double> vector) => new(vector);
    public static Vector Create(double[] vector) => new(vector);
    public static Vector Create(IEnumerable<float> vector) => new(vector);

    public static Vector Parse(string json, IJsonSerializer? serializer = default) => new(
        serializer?.Deserialize<double[]>(json) ??
        JsonSerializer.Deserialize<double[]>(json) ??
        throw new NotSupportedException("Unable to parse json")
        );

    public static Vector? From(object? input) => input switch
    {
        null => null,
        Vector v => v,
        double[] d => Create(d),
        string s => Parse(s),
        BinaryReader r => Read(r),
        _ => throw new NotSupportedException($"{input.GetType()} is not convertible to {nameof(Vector)}")
    };

    public override string ToString() => ToString(default);

    public string ToString(IJsonSerializer? serializer) =>
        serializer?.Serialize(Value) ??
        JsonSerializer.Serialize(Value);

    public static Vector Read(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var vector = new double[length];
        for (var i = 0; i < length; i++)
        {
            vector[i] = reader.ReadDouble();
        }

        var magnitude = reader.BaseStream.Position < reader.BaseStream.Length ?
            reader.ReadDouble() :
            global::System.Math.Sqrt(VectorMath.DotProduct(vector, vector));

        return new()
        {
            Magnitude = magnitude,
            Value = vector,
        };
    }

    public void Write(BinaryWriter writer) => Write(writer, this);

    public static void Write(BinaryWriter writer, Vector vector)
    {
        writer.Write(vector.Value.Length);
        foreach (var value in vector.Value)
        {
            writer.Write(value);
        }
        writer.Write(vector.Magnitude);
    }

    public static implicit operator double[]?(Vector? vector) => vector?.Value;
    public static implicit operator Vector?(double[]? vector) => vector == null ? default : Create(vector);

    public static implicit operator float[]?(Vector? vector) => vector?.Value.Select(Convert.ToSingle).ToArray();
    public static implicit operator Vector?(float[]? vector) => vector == null ? default : Create(vector);

    public static implicit operator string?(Vector? vector) => vector?.ToString();
    public static implicit operator Vector?(string? vector) => vector == null ? default : Parse(vector);
}
