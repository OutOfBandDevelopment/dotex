using OoBDev.System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OoBDev.System.Math;

/// <summary>
/// Represents a mathematical vector with double-precision floating-point values and precomputed magnitude.
/// </summary>
public readonly record struct Vector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> struct from a sequence of single-precision values.
    /// </summary>
    /// <param name="vector">The sequence of float values.</param>
    public Vector(IEnumerable<float> vector) : this(vector.Select(Convert.ToDouble))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> struct from a sequence of double-precision values.
    /// </summary>
    /// <param name="vector">The sequence of double values.</param>
    public Vector(IEnumerable<double> vector) : this(vector.ToArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> struct from an array of double-precision values.
    /// </summary>
    /// <param name="vector">The array of double values.</param>
    public Vector(double[] vector)
    {
        Value = vector;
        Magnitude = global::System.Math.Sqrt(VectorMath.DotProduct(vector, vector));
    }

    /// <summary>
    /// Gets the array of double-precision values representing the vector.
    /// </summary>
    public double[] Value { get; init; }

    /// <summary>
    /// Gets the precomputed magnitude (length) of the vector.
    /// </summary>
    public double Magnitude { get; init; }

    /// <summary>
    /// Calculates the distance between this vector and another vector using the specified metric.
    /// </summary>
    /// <param name="vector">The vector to compare with.</param>
    /// <param name="distanceMetric">The distance metric to use. Default is <see cref="VectorDistanceMetrics.Cosine"/>.</param>
    /// <returns>The calculated distance, or null if the other vector is null.</returns>
    public double? Distance(Vector? vector, VectorDistanceMetrics distanceMetric = VectorDistanceMetrics.Cosine) =>
        VectorMath.Distance(this, vector, distanceMetric);

    /// <summary>
    /// Creates a new vector from a sequence of double-precision values.
    /// </summary>
    /// <param name="vector">The sequence of double values.</param>
    /// <returns>A new <see cref="Vector"/> instance.</returns>
    public static Vector Create(IEnumerable<double> vector) => new(vector);

    /// <summary>
    /// Creates a new vector from an array of double-precision values.
    /// </summary>
    /// <param name="vector">The array of double values.</param>
    /// <returns>A new <see cref="Vector"/> instance.</returns>
    public static Vector Create(double[] vector) => new(vector);

    /// <summary>
    /// Creates a new vector from a sequence of single-precision values.
    /// </summary>
    /// <param name="vector">The sequence of float values.</param>
    /// <returns>A new <see cref="Vector"/> instance.</returns>
    public static Vector Create(IEnumerable<float> vector) => new(vector);

    /// <summary>
    /// Parses a JSON string representing a vector.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="serializer">Optional custom JSON serializer.</param>
    /// <returns>A new <see cref="Vector"/> instance.</returns>
    public static Vector Parse(string json, IJsonSerializer? serializer = default) => new(
        serializer?.Deserialize<double[]>(json) ??
        JsonSerializer.Deserialize<double[]>(json) ??
        throw new NotSupportedException("Unable to parse json")
        );

    /// <summary>
    /// Attempts to convert an object to a <see cref="Vector"/>.
    /// </summary>
    /// <param name="input">The input object to convert.</param>
    /// <returns>A <see cref="Vector"/> instance, or null if the input is null or cannot be converted.</returns>
    public static Vector? From(object? input) => input switch
    {
        null => null,
        Vector v => v,
        double[] d => Create(d),
        string s => Parse(s),
        BinaryReader r => Read(r),
        _ => throw new NotSupportedException($"{input.GetType()} is not convertible to {nameof(Vector)}")
    };

    /// <summary>
    /// Returns a JSON string representation of the vector.
    /// </summary>
    /// <returns>A JSON string.</returns>
    public override string ToString() => ToString(default);

    /// <summary>
    /// Returns a JSON string representation of the vector using an optional custom serializer.
    /// </summary>
    /// <param name="serializer">Optional custom JSON serializer.</param>
    /// <returns>A JSON string.</returns>
    public string ToString(IJsonSerializer? serializer) =>
        serializer?.Serialize(Value) ??
        JsonSerializer.Serialize(Value);

    /// <summary>
    /// Reads a vector from a binary stream.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <returns>A new <see cref="Vector"/> instance.</returns>
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

    /// <summary>
    /// Writes this vector to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    public void Write(BinaryWriter writer) => Write(writer, this);

    /// <summary>
    /// Writes a vector to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <param name="vector">The vector to write.</param>
    public static void Write(BinaryWriter writer, Vector vector)
    {
        writer.Write(vector.Value.Length);
        foreach (var value in vector.Value)
        {
            writer.Write(value);
        }
        writer.Write(vector.Magnitude);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector"/> to a double array.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static implicit operator double[]?(Vector? vector) => vector?.Value;

    /// <summary>
    /// Implicitly converts a double array to a <see cref="Vector"/>.
    /// </summary>
    /// <param name="vector">The double array to convert.</param>
    public static implicit operator Vector?(double[]? vector) => vector == null ? default : Create(vector);

    /// <summary>
    /// Implicitly converts a <see cref="Vector"/> to a float array.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static implicit operator float[]?(Vector? vector) => vector?.Value.Select(Convert.ToSingle).ToArray();

    /// <summary>
    /// Implicitly converts a float array to a <see cref="Vector"/>.
    /// </summary>
    /// <param name="vector">The float array to convert.</param>
    public static implicit operator Vector?(float[]? vector) => vector == null ? default : Create(vector);

    /// <summary>
    /// Implicitly converts a <see cref="Vector"/> to a JSON string.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static implicit operator string?(Vector? vector) => vector?.ToString();

    /// <summary>
    /// Implicitly converts a JSON string to a <see cref="Vector"/>.
    /// </summary>
    /// <param name="vector">The JSON string to convert.</param>
    public static implicit operator Vector?(string? vector) => vector == null ? default : Parse(vector);
}
