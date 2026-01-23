using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR aggregate function that collects single-precision vectors into a matrix where each vector becomes a row.
/// </summary>
[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[VectorToMatrixF]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class VectorToMatrixFAggregate : IBinarySerialize
{
    private int _length;
    private List<SqlVectorF> _vectors = [];

    /// <summary>
    /// Initializes the aggregate state.
    /// </summary>
    public void Init() => _vectors = [];

    /// <summary>
    /// Accumulates a vector to be added as a row in the resulting matrix.
    /// </summary>
    /// <param name="vector">The vector to add.</param>
    /// <exception cref="NotSupportedException">Thrown when vectors have different lengths.</exception>
    public void Accumulate(SqlVectorF vector)
    {
        if (vector.IsNull) return;

        if (_length == 0)
        {
            _length = vector.Values.Count;
        }
        else if (_length != vector.Values.Count)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }

        _vectors.Add(vector);
    }

    /// <summary>
    /// Merges another aggregate instance into this one for parallel execution.
    /// </summary>
    /// <param name="other">The other aggregate instance to merge.</param>
    /// <exception cref="NotSupportedException">Thrown when vectors have different lengths.</exception>
    public void Merge(VectorToMatrixFAggregate other)
    {
        if (_length != other._length)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }

        _vectors.AddRange(other._vectors);
    }

    /// <summary>
    /// Completes the aggregation and returns a matrix with accumulated vectors as rows.
    /// </summary>
    /// <returns>A matrix where each row is one of the accumulated vectors.</returns>
    public SqlMatrixF Terminate()
    {
        var data = new float[_vectors.Count, _length];

        for (var r = 0; r < _vectors.Count; r++)
        {
            var vector = _vectors[r];
            for (var c = 0; c < _length; c++)
            {
                data[r, c] = (float)vector.Values[c];
            }
        }

        return new SqlMatrixF(data);
    }

    /// <summary>
    /// Deserializes the aggregate state from a binary stream.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    public void Read(BinaryReader reader)
    {
        _length = reader.ReadInt32();
        var vectorCount = reader.ReadInt32();

        for (var v = 0; v < vectorCount; v++)
        {
            var vector = new SqlVectorF();
            vector.Read(reader);
            _vectors.Add(vector);
        }
    }

    /// <summary>
    /// Serializes the aggregate state to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    public void Write(BinaryWriter writer)
    {
        writer.Write(_length);
        writer.Write(_vectors.Count);
        foreach (var vector in _vectors)
            vector.Write(writer);
    }
}
