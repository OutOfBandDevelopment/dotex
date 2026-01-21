using Microsoft.SqlServer.Server;
using System;
using System.IO;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR aggregate function that calculates the centroid (mean) of a set of single-precision vectors.
/// </summary>
[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[CentroidF]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class CentroidFAggregate : IBinarySerialize
{
    private double[] _sum;
    private int _count;

    /// <summary>
    /// Initializes the aggregate state.
    /// </summary>
    public void Init()
    {
        _sum = [];
        _count = 0;
    }

    /// <summary>
    /// Accumulates a vector into the running sum.
    /// </summary>
    /// <param name="vector">The vector to add to the aggregate.</param>
    /// <exception cref="NotSupportedException">Thrown when vectors have different lengths.</exception>
    public void Accumulate(SqlVectorF vector)
    {
        if (vector.IsNull) return;

        if (_sum.Length == 0)
        {
            _sum = new double[vector.Values.Count];
        }
        else if (_sum.Length != vector.Values.Count)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }

        for (var i = 0; i < _sum.Length; i++)
        {
            _sum[i] += vector.Values[i];
        }

        _count++;
    }

    /// <summary>
    /// Merges another aggregate instance into this one for parallel execution.
    /// </summary>
    /// <param name="other">The other aggregate instance to merge.</param>
    public void Merge(CentroidFAggregate other)
    {
        if (other != null)
        {
            if (_sum.Length == 0)
            {
                _sum = [.. other._sum];
                _count = other._count;
            }
            else
            {
                for (var i = 0; i < _sum.Length; i++)
                {
                    _sum[i] += other._sum[i];
                }
                _count += other._count;
            }
        }
    }

    /// <summary>
    /// Completes the aggregation and returns the centroid vector.
    /// </summary>
    /// <returns>The centroid vector (mean of all accumulated vectors), or null if no vectors were accumulated.</returns>
    public SqlVectorF Terminate()
    {
        if (_count == 0) return SqlVectorF.Null;

        var centroid = new double[_sum.Length];
        for (var i = 0; i < centroid.Length; i++)
        {
            centroid[i] = _sum[i] / _count;
        }

        return new SqlVectorF(centroid);
    }

    /// <summary>
    /// Deserializes the aggregate state from a binary stream.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    public void Read(BinaryReader reader)
    {
        var vector = new SqlVectorF();
        vector.Read(reader);
        _sum = [.. vector.Values];
        _count = reader.ReadInt32();
    }

    /// <summary>
    /// Serializes the aggregate state to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    public void Write(BinaryWriter writer)
    {
        var vector = new SqlVectorF(_sum);
        vector.Write(writer);
        writer.Write(_count);
    }
}
