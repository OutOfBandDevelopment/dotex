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
    Name = "[embedding].[Matrix]",
    IsByteOrdered = true,
    MaxByteSize = -1)]
public struct SqlMatrix : INullable, IBinarySerialize, IEquatable<SqlMatrix>
{
    private const ushort Version = 0x00;
    private static readonly SqlMatrix _null = new(true, new double[0, 0], Version);
    private static readonly SqlMatrix _empty = new(false, new double[0, 0], Version);

    private ushort _version;
    private bool _isNull;
    private double[,] _values;

    public readonly bool IsNull => _isNull;
    public readonly IReadOnlyMatrix<double> Values => _values.AsReadOnly();

    private SqlMatrix(bool isNull, double[,] data, ushort version)
    {
        _isNull = isNull;
        _values = data;
        _version = version;
    }

    public SqlMatrix(double[,] data) : this(false, data, Version) { }

    public static SqlMatrix Null => _null;
    public static SqlMatrix Empty => _empty;

    [SqlMethod(
        Name = nameof(Row),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlVector Row(SqlInt16 row) => row.IsNull ? SqlVector.Null : new([.. Values.Row(row.Value)]);

    [SqlMethod(
        Name = nameof(Column),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlVector Column(SqlInt16 column)
    {
        if (column.IsNull) return SqlVector.Null;

        var realColumn = column.Value;
        var columns = (short)_values.GetUpperBound(1) + 1;
        if (column.Value >= columns) throw new ArgumentOutOfRangeException(nameof(column));
        var rows = (short)_values.GetUpperBound(0) + 1;

        var data = new double[rows];
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
            _values = new double[0, 0];
            return;
        }

        _isNull = false;
        var data = new double[rows, columns];
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
            rows = (short)(_values.GetUpperBound(0) + 1);
            columns = (short)(_values.GetUpperBound(1) + 1);
        }

        writer.Write(rows);
        writer.Write(columns);

        for (var c = 0; c < columns; c++)
            for (var r = 0; r < rows; r++)
            {
                writer.Write(_values[r, c]);
            }
    }

    public static SqlMatrix Parse(SqlString input)
    {
        if (input.IsNull) return Null;

        var rowStrings = input.Value.Split(['\n', '\r', '|'], options: StringSplitOptions.RemoveEmptyEntries);
        var rows = rowStrings.Length;

        double[,] data = default;
        for (var r = 0; r < rows; r++)
        {
            var columnStrings = rowStrings[r].Split(['\t', ','], options: StringSplitOptions.RemoveEmptyEntries);
            var columns = columnStrings.Length;
            if (r == 0)
            {
                data = new double[rows, columns];
            }

            for (var c = 0; c < columns; c++)
            {
                data[r, c] = double.Parse(columnStrings[c], CultureInfo.InvariantCulture);
            }
        }

        return new SqlMatrix(data);
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
        other is SqlMatrix matrix && Equals(matrix);

    public readonly bool Equals(SqlMatrix other)
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
