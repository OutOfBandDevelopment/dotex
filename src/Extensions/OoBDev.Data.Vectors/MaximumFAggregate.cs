using Microsoft.SqlServer.Server;
using System;
using System.IO;

namespace OoBDev.Data.Vectors;

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

    public void Init()
    {
        _sum = [];
    }

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

    public SqlVectorF Terminate()
    {
        var data = new double[_sum.Length];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = _sum[i];
        }

        return new SqlVectorF(data);
    }

    public void Read(BinaryReader reader)
    {
        var vector = new SqlVectorF();
        vector.Read(reader);
        _sum = [.. vector.Values];
    }

    public void Write(BinaryWriter writer)
    {
        var vector = new SqlVectorF(_sum);
        vector.Write(writer);
    }
}
