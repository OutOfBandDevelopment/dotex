using Microsoft.SqlServer.Server;
using System;
using System.IO;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR aggregate function that computes the element-wise maximum across a set of single-precision vectors.
/// </summary>
[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[MaximumF]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class MaximumFAggregate : IBinarySerialize
{
    private double[] _sum;

    /// <summary>
    /// Initializes the aggregate state.
    /// </summary>
    public void Init() => _sum = [];

    /// <summary>
    /// Accumulates a vector, keeping the maximum value for each element position.
    /// </summary>
    /// <param name="vector">The vector to accumulate.</param>
    /// <exception cref="NotSupportedException">Thrown when vectors have different lengths.</exception>
    public void Accumulate(SqlVectorF vector)
    {
        if (vector.IsNull) return;

        if (_sum.Length == 0)
        {
            _sum = [.. vector.Values];
        }
        else if (_sum.Length != vector.Values.Count)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }
        else
        {
            for (var i = 0; i < _sum.Length; i++)
            {
                if (vector.Values[i] > _sum[i])
                    _sum[i] = vector.Values[i];
            }
        }
    }

    /// <summary>
    /// Merges another aggregate instance into this one for parallel execution.
    /// </summary>
    /// <param name="other">The other aggregate instance to merge.</param>
    public void Merge(MaximumFAggregate other)
    {
        if (other != null)
        {
            if (_sum.Length == 0)
            {
                _sum = [.. other._sum];
            }
            else
            {
                for (var i = 0; i < _sum.Length; i++)
                {
                    _sum[i] += other._sum[i];
                }
            }
        }
    }

    /// <summary>
    /// Completes the aggregation and returns the vector of maximum values.
    /// </summary>
    /// <returns>A vector containing the maximum value at each element position.</returns>
    public SqlVectorF Terminate()
    {
        var data = new double[_sum.Length];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = _sum[i];
        }

        return new SqlVectorF(data);
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
    }

    /// <summary>
    /// Serializes the aggregate state to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    public void Write(BinaryWriter writer)
    {
        var vector = new SqlVectorF(_sum);
        vector.Write(writer);
    }
}
