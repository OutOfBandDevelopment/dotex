using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Text;

namespace OoBDev.Data.Vectors;

[Serializable]
[SqlUserDefinedType(
    Format.UserDefined,
    Name = "[embedding].[Vector]",
    IsByteOrdered = true,
    MaxByteSize = -1)]
public struct SqlMatrixF : INullable, IBinarySerialize, IEquatable<SqlMatrixF>
{
    private const ushort Version = 0x00;
    private static readonly SqlMatrixF _null = new(true, new float[0, 0], Version);
    private static readonly SqlMatrixF _empty = new(false, new float[0, 0], Version);

    private ushort _version;
    private bool _isNull;
    private float[,] _values;

    public readonly bool IsNull => _isNull;
    public readonly IReadOnlyMatrix<float> Values => _values.AsReadOnly();

    private SqlMatrixF(bool isNull, float[,] data, ushort version)
    {
        _isNull = isNull;
        _values = data;
        _version = version;
    }

    public SqlMatrixF(float[,] data) : this(false, data, Version) { }

    public static SqlMatrixF Null => _null;
    public static SqlMatrixF Empty => _empty;

    [SqlMethod(
        Name = nameof(Row),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlVectorF Row(SqlInt16 row) => row.IsNull ? SqlVectorF.Null : new([.. Values.Row(row.Value)]);

    [SqlMethod(
        Name = nameof(Column),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlVectorF Column(SqlInt16 column)
    {
        if (column.IsNull) return SqlVectorF.Null;

        var realColumn = column.Value;
        var columns = (short)_values.GetUpperBound(1) + 1;
        if (column.Value >= columns) throw new ArgumentOutOfRangeException(nameof(column));
        var rows = (short)_values.GetUpperBound(0) + 1;

        var data = new float[rows];
        for (var r = 0; r < rows; r++)
        {
            data[r] = _values[r, realColumn];
        }
        return new(data);
    }

    [SqlMethod(
        Name = nameof(Element),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Element(SqlInt16 row, SqlInt16 column) =>
        (IsNull || row.IsNull || column.IsNull) ? SqlSingle.Null : (SqlSingle)Values.Get(row.Value, column.Value);

    public void Read(BinaryReader reader)
    {
        _version = reader.ReadUInt16();

        var rows = reader.ReadInt16();
        var columns = reader.ReadInt16();
        if (rows == -1 || columns == -1)
        {
            _isNull = true;
            _values = new float[0, 0];
            return;
        }

        _isNull = false;
        var data = new float[rows, columns];
        for (var c = 0; c < columns; c++)
            for (var r = 0; r < rows; r++)
            {
                data[r, c] = reader.ReadSingle();
            }
        _values = data;
    }

    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(Version);

        short rows, columns;

        if (_isNull)
        {
            rows = -1;
            columns = -1;
        }
        else
        {
            rows = (short)_values.GetUpperBound(0);
            columns = (short)_values.GetUpperBound(1);
        }

        writer.Write(rows);
        writer.Write(columns);

        for (var c = 0; c < columns; c++)
            for (var r = 0; r < rows; r++)
            {
                writer.Write(_values[r, c]);
            }
    }

    public static SqlMatrixF Parse(SqlString input)
    {
        if (input.IsNull) return Null;

        var rowStrings = input.Value.Split(['\n', '\r', '|'], options: StringSplitOptions.RemoveEmptyEntries);
        var rows = rowStrings.Length;

        float[,] data = default;
        for (var r = 0; r < rows; r++)
        {
            var columnStrings = rowStrings[r].Split(['\t', ','], options: StringSplitOptions.RemoveEmptyEntries);
            var columns = columnStrings.Length;
            if (r == 0)
            {
                data = new float[rows, columns];
            }

            for (var c = 0; c < columns; c++)
            {
                data[r, c] = float.Parse(columnStrings[c], CultureInfo.InvariantCulture);
            }
        }

        return new SqlMatrixF(data);
    }

    public override readonly string ToString()
    {
        var sb = new StringBuilder();

        var rows = (short)_values.GetUpperBound(0) + 1;
        var columns = (short)_values.GetUpperBound(1) + 1;

        for (var r = 0; r < rows; r++)
        {
            if (r != 0) sb.Append('\n');
            for (var c = 0; c < columns; c++)
            {
                if (c != 0) sb.Append('\t');
                sb.Append(_values[r, c].ToString("e7", CultureInfo.InvariantCulture));
            }
        }
        return sb.ToString();
    }

    public override readonly bool Equals(object other) =>
        other is SqlMatrixF matrix && Equals(matrix);

    public readonly bool Equals(SqlMatrixF other)
    {
        if (IsNull != other.IsNull) return false;

        var rows = _values.GetUpperBound(0) + 1;
        var columns = _values.GetUpperBound(1) + 1;

        if (rows != other.Values.Rows || columns != other.Values.Columns) return false;

        for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                if (_values[r, c] != other.Values[r, c]) return false;
            }
        return true;
    }

}
