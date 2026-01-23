using Microsoft.SqlServer.Server;
using System;
using System.IO;

namespace OoBDev.Data.Vectors;

/// <summary>
/// SQL CLR aggregate function that computes the element-wise minimum across a set of double-precision vectors.
/// </summary>
[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[Minimum]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class MinimumAggregate : IBinarySerialize
{
    private double[] _sum;

    /// <summary>
    /// Initializes the aggregate state.
    /// </summary>
    public void Init() => _sum = [];

    /// <summary>
    /// Accumulates a vector, keeping the minimum value for each element position.
    /// </summary>
    /// <param name="vector">The vector to accumulate.</param>
    /// <exception cref="NotSupportedException">Thrown when vectors have different lengths.</exception>
    public void Accumulate(SqlVector vector)
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
                if (vector.Values[i] < _sum[i])
                    _sum[i] = vector.Values[i];
            }
        }
    }

    /// <summary>
    /// Merges another aggregate instance into this one for parallel execution.
    /// </summary>
    /// <param name="other">The other aggregate instance to merge.</param>
    public void Merge(MinimumAggregate other)
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
    /// Completes the aggregation and returns the vector of minimum values.
    /// </summary>
    /// <returns>A vector containing the minimum value at each element position.</returns>
    public SqlVector Terminate()
    {
        var data = new double[_sum.Length];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = _sum[i];
        }

        return new SqlVector(data);
    }

    /// <summary>
    /// Deserializes the aggregate state from a binary stream.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    public void Read(BinaryReader reader)
    {
        var vector = new SqlVector();
        vector.Read(reader);
        _sum = [.. vector.Values];
    }

    /// <summary>
    /// Serializes the aggregate state to a binary stream.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    public void Write(BinaryWriter writer)
    {
        var vector = new SqlVector(_sum);
        vector.Write(writer);
    }
}
