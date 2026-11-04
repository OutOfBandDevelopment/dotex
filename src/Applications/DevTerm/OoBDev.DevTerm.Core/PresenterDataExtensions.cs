using System;
using System.Buffers;

namespace OoBDev.DevTerm;

/// <summary>Helpers for presenters consuming a <see cref="ReadOnlySequence{T}"/> without forcing a copy in the common case.</summary>
public static class PresenterDataExtensions
{
    /// <summary>Returns the bytes as one span: zero-copy for a single segment, a copy otherwise.</summary>
    public static ReadOnlySpan<byte> ToContiguousSpan(this in ReadOnlySequence<byte> data) =>
        data.IsSingleSegment ? data.FirstSpan : data.ToArray();
}
