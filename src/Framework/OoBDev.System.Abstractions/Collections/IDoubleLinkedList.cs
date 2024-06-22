namespace OoBDev.System.Collections;

/// <summary>
/// interface for a double linked list
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDoubleLinkedList<T>
{
    /// <summary>
    /// previous segment for double linked list
    /// </summary>
    IDoubleLinkedList<T>? Previous { get; }
    /// <summary>
    /// current element
    /// </summary>
    T Current { get; }
    /// <summary>
    /// next segment for double linked list
    /// </summary>
    IDoubleLinkedList<T>? Next { get; }

    /// <summary>
    /// Inserts a new element after the current element.
    /// </summary>
    /// <param name="item">The element to insert after the current element.</param>
    /// <returns>The new segment that contains the inserted element.</returns>
    /// <remarks>
    /// This method will create a new segment with the specified item and insert it
    /// after the current segment. The current segment's <see cref="Next"/> property
    /// will be updated to point to the new segment, and the new segment's <see cref="Previous"/>
    /// property will point to the current segment.
    /// </remarks>
    IDoubleLinkedList<T> InsertBefore(T item);

    /// <summary>
    /// Inserts a new element after the current element.
    /// </summary>
    /// <param name="item">The element to insert after the current element.</param>
    /// <returns>The new segment that contains the inserted element.</returns>
    /// <remarks>
    /// This method will create a new segment with the specified item and insert it
    /// after the current segment. The current segment's <see cref="Next"/> property
    /// will be updated to point to the new segment, and the new segment's <see cref="Previous"/>
    /// property will point to the current segment.
    /// </remarks>
    IDoubleLinkedList<T> InsertAfter(T item);

    /// <summary>
    /// Gets the position of the current element in the linked list.
    /// </summary>
    /// <remarks>
    /// The position is a zero-based index representing the element's location in the list.
    /// It can be used to determine the order of elements within the linked list.
    /// </remarks>
    int Position { get; }
}
