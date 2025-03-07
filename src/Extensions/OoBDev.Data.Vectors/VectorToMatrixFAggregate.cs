using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Data.Vectors;

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

    public void Init()
    {
        _vectors = [];
    }

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

    public void Merge(VectorToMatrixFAggregate other)
    {
        if (_length != other._length)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }

        _vectors.AddRange(other._vectors);
    }

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

    public void Write(BinaryWriter writer)
    {
        writer.Write(_length);
        writer.Write(_vectors.Count);
        foreach (var vector in _vectors)
            vector.Write(writer);
    }
}
