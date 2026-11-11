using System;
using System.Collections.Generic;

namespace OoBDev.EventSourcing;

/// <summary>
/// Base class of an event-sourced aggregate: state changes only by raising events, which are applied to the
/// state at once and kept until the repository saves them.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<object> _uncommitted = [];

    /// <summary>
    /// Gets the aggregate identifier (set by the first event or by the repository when loading).
    /// </summary>
    public string Id { get; protected set; } = "";

    /// <summary>
    /// Gets the sequence of the last event saved to the store (0 for a new aggregate).
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Gets the events raised since the last save.
    /// </summary>
    public IReadOnlyList<object> UncommittedEvents => _uncommitted;

    /// <summary>
    /// Changes the state for one event. It must only set fields and never throw or have side effects, because it also runs when the history is replayed.
    /// </summary>
    /// <param name="event">The event.</param>
    protected abstract void Apply(object @event);

    /// <summary>
    /// Applies a new event and keeps it for the next save.
    /// </summary>
    /// <param name="event">The event.</param>
    protected void Raise(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        Apply(@event);
        _uncommitted.Add(@event);
    }

    /// <summary>
    /// Rebuilds the state from saved events (used by the repository).
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="history">The saved events in order.</param>
    /// <param name="version">The sequence of the last saved event.</param>
    public void LoadFromHistory(string id, IEnumerable<object> history, long version)
    {
        ArgumentNullException.ThrowIfNull(history);
        Id = id;
        foreach (var @event in history)
            Apply(@event);
        Version = version;
        _uncommitted.Clear();
    }

    /// <summary>
    /// Clears the uncommitted events after a successful save (used by the repository).
    /// </summary>
    /// <param name="version">The sequence of the last saved event.</param>
    public void MarkCommitted(long version)
    {
        Version = version;
        _uncommitted.Clear();
    }
}
