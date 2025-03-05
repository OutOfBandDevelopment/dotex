using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

internal class ReadOnlyMatrix<T> : IReadOnlyMatrix<T>
{
    private readonly T[,] _values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMatrix(T[,] values) => _values = values;

    public T this[int row, int column] => Get(row, column);

    public int Count => Rows;

    public int Rows => _values.GetUpperBound(0) + 1;

    public int Columns => _values.GetUpperBound(1) + 1;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyCollection<T> Column(int column)
    {
        var columns = Columns;
        if (column >= columns) throw new ArgumentOutOfRangeException(nameof(column));
        var rows = Rows;

        var data = new T[rows];
        for (var r = 0; r <= rows; r++)
        {
            data[r] = _values[r, column];
        }
        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int row, int column) => _values[row, column];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyCollection<T> Row(int row)
    {
        var rows = Rows;
        if (row >= rows) throw new ArgumentOutOfRangeException(nameof(row));
        var columns = Columns;

        var data = new T[columns];
        for (var c = 0; c <= columns; c++)
        {
            data[c] = _values[row, c];
        }
        return data;
    }

    public IEnumerator<IReadOnlyCollection<T>> GetEnumerator()
    {
        var rows = Rows;
        for (var r = 0; r < rows; r++)
            yield return Row(r);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
