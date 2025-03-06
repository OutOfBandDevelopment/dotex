using System.Runtime.CompilerServices;

namespace OoBDev.Data.Vectors;

public static class ArrayExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyMatrix<T> AsReadOnly<T>(this T[,] values) => new ReadOnlyMatrix<T>(values);
}
