using System;

namespace OoBDev.System.Collections;

internal class DoubleLinkedList<T>(T item) : IDoubleLinkedList<T>
{
    private readonly object _lock = new();

    public IDoubleLinkedList<T>? Previous { get; private set; }
    public T Current { get; } = item;
    public IDoubleLinkedList<T>? Next { get; private set; }

    public int Position { get; private set; }

    private static void SyncPosition(DoubleLinkedList<T> from)
    {
        var seed = from.Position;
        while (from?.Next is DoubleLinkedList<T> next)
        {
            next.Position = ++seed;
            from = next;
        }
    }

    public IDoubleLinkedList<T> InsertBefore(T item)
    {
        lock (_lock)
        {
            var newItem = new DoubleLinkedList<T>(item) { Previous = Previous, Next = this };
            if (Previous is DoubleLinkedList<T> previous)
            {
                previous.Next = newItem;
                newItem.Position = previous.Position + 1;
            }
            else if (Previous != null)
            {
                throw new NotSupportedException();
            }
            Previous = newItem;
            SyncPosition(newItem);
            return newItem;
        }
    }
    public IDoubleLinkedList<T> InsertAfter(T item)
    {
        lock (_lock)
        {
            var newItem = new DoubleLinkedList<T>(item) { Previous = this, Next = Next, Position = Position + 1, };
            if (Next is DoubleLinkedList<T> next)
            {
                next.Previous = newItem;
            }
            else if (Next != null)
            {
                throw new NotSupportedException();
            }
            Next = newItem;
            SyncPosition(newItem);
            return newItem;
        }
    }
}
