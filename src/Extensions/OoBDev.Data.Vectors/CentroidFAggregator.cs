using Microsoft.SqlServer.Server;
using System;
using System.IO;

namespace OoBDev.Data.Vectors;

[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[CentroidF]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class CentroidFAggregator : IBinarySerialize
{
    private double[] _sum;
    private int _count;

    public void Init()
    {
        _sum = [];
        _count = 0;
    }

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

    public void Merge(CentroidFAggregator other)
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

    public void Read(BinaryReader reader)
    {
        var vector = new SqlVectorF();
        vector.Read(reader);
        _sum = [.. vector.Values];
        _count = reader.ReadInt32();
    }

    public void Write(BinaryWriter writer)
    {
        var vector = new SqlVectorF(_sum);
        vector.Write(writer);
        writer.Write(_count);
    }
}
