using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Text;

namespace OoBDev.Data.Vectors;

/// <summary>
/// Represents a SQL Server CLR user-defined type for storing and manipulating single-precision (float) matrices.
/// Used for matrix storage in SQL databases, particularly for embeddings and machine learning applications where reduced precision is acceptable.
/// </summary>
[Serializable]
[SqlUserDefinedType(
    Format.UserDefined,
    Name = "[embedding].[MatrixF]",
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

    /// <summary>
    /// Gets a value indicating whether this matrix instance represents a SQL NULL value.
    /// </summary>
    public readonly bool IsNull => _isNull;

    /// <summary>
    /// Gets the matrix data as a read-only collection of single-precision values.
    /// </summary>
    public readonly IReadOnlyMatrix<float> Values => _values.AsReadOnly();

    private SqlMatrixF(bool isNull, float[,] data, ushort version)
    {
        _isNull = isNull;
        _values = data;
        _version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMatrixF"/> struct with the specified data.
    /// </summary>
    /// <param name="data">The two-dimensional array of single-precision values for the matrix.</param>
    public SqlMatrixF(float[,] data) : this(false, data, Version) { }

    /// <summary>
    /// Gets a SQL NULL matrix instance.
    /// </summary>
    public static SqlMatrixF Null => _null;

    /// <summary>
    /// Gets an empty matrix instance with zero rows and columns.
    /// </summary>
    public static SqlMatrixF Empty => _empty;

    /// <summary>
    /// Extracts a specific row from the matrix as a vector.
    /// </summary>
    /// <param name="row">The zero-based row index to extract.</param>
    /// <returns>A <see cref="SqlVectorF"/> containing the values from the specified row, or SQL NULL if the row parameter is NULL.</returns>
    [SqlMethod(
        Name = nameof(Row),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlVectorF Row(SqlInt16 row) => row.IsNull ? SqlVectorF.Null : new([.. Values.Row(row.Value)]);

    /// <summary>
    /// Extracts a specific column from the matrix as a vector.
    /// </summary>
    /// <param name="column">The zero-based column index to extract.</param>
    /// <returns>A <see cref="SqlVectorF"/> containing the values from the specified column, or SQL NULL if the column parameter is NULL.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the column index is greater than or equal to the number of columns in the matrix.</exception>
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

    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    /// <returns>The row count as a <see cref="SqlInt16"/>.</returns>
    [SqlMethod(
        Name = nameof(Rows),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlInt16 Rows() => (short)Values.Rows;

    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    /// <returns>The column count as a <see cref="SqlInt16"/>.</returns>
    [SqlMethod(
        Name = nameof(Columns),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public readonly SqlInt16 Columns() => (short)Values.Columns;

    /// <summary>
    /// Retrieves a single element from the matrix at the specified row and column indices.
    /// </summary>
    /// <param name="row">The zero-based row index.</param>
    /// <param name="column">The zero-based column index.</param>
    /// <returns>The element value as a <see cref="SqlSingle"/>, or SQL NULL if this matrix or either parameter is NULL.</returns>
    [SqlMethod(
        Name = nameof(Element),
        OnNullCall = false,
        IsDeterministic = true,
        IsPrecise = true,
        IsMutator = false
        )]
    public SqlSingle Element(SqlInt16 row, SqlInt16 column) =>
        (IsNull || row.IsNull || column.IsNull) ? SqlSingle.Null : (SqlSingle)Values.Get(row.Value, column.Value);

    /// <summary>
    /// Deserializes the matrix from binary format. Used by SQL Server CLR for data retrieval.
    /// </summary>
    /// <param name="reader">The binary reader containing the serialized matrix data.</param>
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

    /// <summary>
    /// Serializes the matrix to binary format. Used by SQL Server CLR for data storage.
    /// </summary>
    /// <param name="writer">The binary writer to write the serialized matrix data to.</param>
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

    /// <summary>
    /// Parses a string representation of a matrix into a <see cref="SqlMatrixF"/> instance.
    /// Supports multiple row and column separators (newline, pipe, tab, comma).
    /// </summary>
    /// <param name="input">The string representation of the matrix to parse.</param>
    /// <returns>A new <see cref="SqlMatrixF"/> containing the parsed values, or SQL NULL if the input is NULL.</returns>
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

    /// <summary>
    /// Converts the matrix to its string representation using scientific notation (e7 format).
    /// Rows are separated by newlines, columns are separated by tabs.
    /// </summary>
    /// <returns>A string representation of the matrix values.</returns>
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

    /// <summary>
    /// Determines whether the specified object is equal to the current matrix.
    /// </summary>
    /// <param name="other">The object to compare with the current matrix.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="SqlMatrixF"/> and has the same dimensions and element values; otherwise, <c>false</c>.</returns>
    public override readonly bool Equals(object other) =>
        other is SqlMatrixF matrix && Equals(matrix);

    /// <summary>
    /// Determines whether the specified matrix is equal to the current matrix.
    /// Compares dimensions and all element values for equality.
    /// </summary>
    /// <param name="other">The matrix to compare with the current matrix.</param>
    /// <returns><c>true</c> if the matrices have the same dimensions and all corresponding elements are equal; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Calculates a hash code for the matrix based on its null state and all element values.
    /// </summary>
    /// <returns>A hash code for the current matrix.</returns>
    public override readonly int GetHashCode()
    {
        var hash = _isNull.GetHashCode() * 31;

        var rows = _values.GetUpperBound(0) + 1;
        var columns = _values.GetUpperBound(1) + 1;

        for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                hash += Values[r, c].GetHashCode() * 47;
            }

        return hash;
    }

    /// <summary>
    /// Determines whether two matrices are equal by comparing their dimensions and element values.
    /// </summary>
    /// <param name="left">The first matrix to compare.</param>
    /// <param name="right">The second matrix to compare.</param>
    /// <returns><c>true</c> if the matrices are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(SqlMatrixF left, SqlMatrixF right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    /// <param name="left">The first matrix to compare.</param>
    /// <param name="right">The second matrix to compare.</param>
    /// <returns><c>true</c> if the matrices are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(SqlMatrixF left, SqlMatrixF right) => !(left == right);
}

