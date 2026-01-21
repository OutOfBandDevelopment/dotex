using OoBDev.System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.System.Linq;

/// <summary>
/// Provides extension methods for converting ITuple instances into arrays and lists.
/// </summary>
public static class TupleExtensions
{
    /// <summary>
    /// Converts an ITuple into an array of objects.
    /// </summary>
    /// <param name="tuple">The tuple to convert.</param>
    /// <returns>An array containing all elements from the tuple as objects.</returns>
    public static object?[] ToArray(this ITuple tuple) =>
        [.. Enumerable.Range(0, tuple.Length).Select(i => tuple[i])];

    /// <summary>
    /// Converts an ITuple into a typed array.
    /// </summary>
    /// <typeparam name="T">The type to cast tuple elements to.</typeparam>
    /// <param name="tuple">The tuple to convert.</param>
    /// <returns>An array containing all elements from the tuple cast to type T.</returns>
    public static T?[] ToArray<T>(this ITuple tuple) =>
        [.. Enumerable.Range(0, tuple.Length).Select(i => (T?)tuple[i])];

    /// <summary>
    /// Converts an ITuple into a typed read-only list.
    /// </summary>
    /// <typeparam name="T">The type to cast tuple elements to.</typeparam>
    /// <param name="tuple">The tuple to convert.</param>
    /// <returns>A read-only list containing all elements from the tuple cast to type T.</returns>
    public static IReadOnlyList<T?> ToList<T>(this ITuple tuple) =>
        Enumerable.Range(0, tuple.Length).Select(i => (T?)tuple[i]).ToArray();
}
