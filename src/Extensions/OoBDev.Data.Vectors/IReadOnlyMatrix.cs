using System.Collections;
using System.Collections.Generic;

namespace OoBDev.Data.Vectors;

/// <summary>
/// Represents a read-only matrix of values.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IReadOnlyMatrix<out T> : IEnumerable<IReadOnlyCollection<T>>, IEnumerable, IReadOnlyCollection<IReadOnlyCollection<T>>
{
    /// <summary>
    /// Gets the element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The element at the specified position.</returns>
    T this[int row, int column] { get; }
    /// <summary>
    /// Gets the element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The element at the specified position.</returns>
    T Get(int row, int column);
    /// <summary>
    /// Gets all elements in the specified row.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <returns>A collection of elements in the row.</returns>
    IReadOnlyCollection<T> Row(int row);
    /// <summary>
    /// Gets all elements in the specified column.
    /// </summary>
    /// <param name="column">The column index.</param>
    /// <returns>A collection of elements in the column.</returns>
    IReadOnlyCollection<T> Column(int column);

    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    int Rows { get; }
    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    int Columns { get; }
}
