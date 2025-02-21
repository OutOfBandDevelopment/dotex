using System.Collections.Generic;

namespace OoBDev.Extensions.Linq;

/// <summary>
/// Reusable extensions for Generic Dictionaries
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// extend try get value to allow for using a different IEqualityComparer{TKey}
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool TryGetValue<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        out TValue? value,
        IEqualityComparer<TKey> comparer
        )
        where TKey : notnull =>
        dictionary.ChangeComparer(comparer).TryGetValue(key, out value);

    /// <summary>
    /// Rebuild dictionary to use a different IEqualityComparer{TKey}
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ChangeComparer<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        IEqualityComparer<TKey> comparer
        )
        where TKey : notnull =>
        new Dictionary<TKey, TValue>(dictionary, comparer);
}
