using System.Collections.Generic;

namespace OoBDev.System.Collections;

/// <summary>
/// Provides extension methods for LinkedList collections.
/// </summary>
public static class LinkedListEx
{
    /// <summary>
    /// Enumerates the linked list in reverse order, from last to first element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the linked list.</typeparam>
    /// <param name="current">The linked list to enumerate in reverse.</param>
    /// <returns>An enumerable that yields elements from the end to the beginning of the linked list.</returns>
    public static IEnumerable<T> AsEnumerableReversed<T>(this LinkedList<T> current)
    {
        var item = current.Last;
        if (item == null) yield break;
        do
        {
            yield return item.Value;
            item = item.Previous;
        }
        while (item?.Previous != null);
    }
}
