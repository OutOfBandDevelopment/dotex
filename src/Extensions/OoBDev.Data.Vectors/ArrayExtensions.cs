using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

/// <summary>
/// Extension methods for array operations.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Converts a two-dimensional array to a read-only matrix.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="values">The two-dimensional array.</param>
    /// <returns>A read-only matrix view of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyMatrix<T> AsReadOnly<T>(this T[,] values) => new ReadOnlyMatrix<T>(values);
}
