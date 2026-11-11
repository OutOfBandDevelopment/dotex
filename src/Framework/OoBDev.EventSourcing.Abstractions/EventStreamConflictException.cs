using System;

namespace OoBDev.EventSourcing;

/// <summary>
/// An append found a newer sequence in the stream than the writer expected (optimistic concurrency).
/// </summary>
/// <param name="streamId">The stream.</param>
/// <param name="expectedSequence">The sequence the writer expected to be last.</param>
public sealed class EventStreamConflictException(string streamId, long expectedSequence)
    : Exception($"Stream '{streamId}' changed after sequence {expectedSequence}.")
{
    /// <summary>
    /// Gets the stream.
    /// </summary>
    public string StreamId { get; } = streamId;

    /// <summary>
    /// Gets the expected last sequence.
    /// </summary>
    public long ExpectedSequence { get; } = expectedSequence;
}
