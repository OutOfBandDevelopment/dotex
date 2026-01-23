using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OoBDev.System.Collections;

/// <summary>
/// this is a enumerator is bidirectional
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Wrap existing IEnumerator
/// </remarks>
/// <param name="base"></param>
[DebuggerDisplay("{Current.ToString()}::{_position}")]
public class ReversableEnumerator<T>(IEnumerator<T> @base) : IReversibleEnumerator<T>
{
    private const int ResetPosition = -1;
    private readonly object _lock = new();
    private IDoubleLinkedList<T>? _pointer = null;
    private bool _reset = false;
    private bool _end = false;

    /// <summary>
    /// Gets the current position in the enumeration sequence. Returns -1 when reset.
    /// </summary>
    public int Position { get; private set; } = ResetPosition;

    /// <summary>
    /// Wrap existing IEnumerable
    /// </summary>
    /// <param name="base"></param>
    public ReversableEnumerator(IEnumerable<T> @base) : this(@base.GetEnumerator()) { }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    public T Current => _pointer == null ? @base.Current : _pointer.Current;

#pragma warning disable CS8603 // Possible null reference return.
    object IEnumerator.Current => Current;
#pragma warning restore CS8603 // Possible null reference return.

    /// <summary>
    /// free any underlying resources
    /// </summary>
    public void Dispose() => @base.Dispose();

    /// <summary>
    /// allow playing to end of current state before checking for new values in enumerable set.
    /// </summary>
    /// <returns>true if advanced</returns>
    public bool MoveNext()
    {
        lock (_lock)
        {
            if (_end)
            {
                return false;
            }
            if (_reset && _pointer != null)
            {
                _reset = false;
                Position++;
                return true;
            }

            if (_pointer == null)
            {
                var advanceBase = @base.MoveNext();
                if (advanceBase)
                {
                    _pointer = new DoubleLinkedList<T>(@base.Current);
                    Position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var next = _pointer.Next;
                if (next != null)
                {
                    _pointer = next;
                    Position++;
                    return true;
                }
                else
                {
                    var advanceBase = @base.MoveNext();
                    if (advanceBase)
                    {
                        _pointer = _pointer.InsertAfter(@base.Current);
                        Position++;
                        return true;
                    }
                    else
                    {
                        _end = true;
                        return false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Moves to the most recent (latest) position in the cached enumeration.
    /// This operation fast-forwards to the end of the currently cached items without pulling new items from the base enumerator.
    /// </summary>
    /// <returns>True if the move was successful; false if there is no cached data.</returns>
    public bool MoveCurrent()
    {
        lock (_lock)
        {
            if (_pointer != null)
            {
                var next = _pointer.FastForward();
                _pointer = next;
                if (_pointer is DoubleLinkedList<T> dd)
                {
                    Position = dd.Position;
                }
                _reset = false;
                _end = false;
                return true;
            }
            else
            {
                _reset = true;
                _end = false;
                return false;
            }
        }
    }

    /// <summary>
    /// if the enumerator has been advanced it may be stepped back here.
    /// </summary>
    /// <returns>true if stepped back</returns>
    public bool MovePrevious()
    {
        lock (_lock)
        {
            var moveTo = _pointer?.Previous;
            if (moveTo == null) return false;
            _pointer = moveTo;
            Position--;
            if (Position < 0) Position = 0;
            return true;
        }
    }

    /// <summary>
    /// if the rewind to the beginning.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            if (_pointer != null)
            {
                _pointer = _pointer?.Rewind();
                _reset = true;
                _end = false;
                Position = ResetPosition;
            }
        }
    }
}
