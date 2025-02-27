using OoBDev.System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.System.Linq;

public static class TupleExtensions
{
    public static object[] ToArray(this ITuple tuple) =>
        [.. Enumerable.Range(0, tuple.Length).Select(i => tuple[i])];
    public static T[] ToArray<T>(this ITuple tuple) =>
        [.. Enumerable.Range(0, tuple.Length).Select(i => (T)tuple[i])];
    public static IReadOnlyList<T> ToList<T>(this ITuple tuple) =>
        Enumerable.Range(0, tuple.Length).Select(i => (T)tuple[i]).ToArray();
}
