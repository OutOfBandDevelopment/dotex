using System.Collections;
using System.Collections.Generic;

namespace OoBDev.Data.Vectors;

public interface IReadOnlyMatrix<out T> : IEnumerable<IReadOnlyCollection<T>>, IEnumerable, IReadOnlyCollection<IReadOnlyCollection<T>>
{
    T this[int row, int column] { get; }
    T Get(int row, int column);
    IReadOnlyCollection<T> Row(int row);
    IReadOnlyCollection<T> Column(int column);

    int Rows { get; }
    int Columns { get; }
}
