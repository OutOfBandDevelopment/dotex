using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Data.Vectors;

[SqlUserDefinedAggregate(
    Format.UserDefined,
    Name = "[embedding].[VectorToMatrix]",
    IsInvariantToDuplicates = false,
    IsInvariantToNulls = true,
    IsInvariantToOrder = true,
    IsNullIfEmpty = true,
    MaxByteSize = -1
    )]
public class VectorToMatrixAggregate : IBinarySerialize
{
    private int _length;
    private List<SqlVector> _vectors = [];

    public void Init()
    {
        _vectors = [];
    }

    public void Accumulate(SqlVector vector)
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

    public void Merge(VectorToMatrixAggregate other)
    {
        if (_length != other._length)
        {
            throw new NotSupportedException($"Vectors must be of the same length");
        }

        _vectors.AddRange(other._vectors);
    }

    public SqlMatrix Terminate()
    {
        var data = new double[_vectors.Count, _length];

        for (var r = 0; r < _vectors.Count; r++)
        {
            var vector = _vectors[r];
            for (var c = 0; c < _length; c++)
            {
                data[r, c] = vector.Values[c];
            }
        }

        return new SqlMatrix(data);
    }

    public void Read(BinaryReader reader)
    {
        _length = reader.ReadInt32();
        var vectorCount = reader.ReadInt32();

        for (var v = 0; v < vectorCount; v++)
        {
            var vector = new SqlVector();
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
