using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.EventSourcing;

/// <summary>
/// Runs the registered <see cref="IEventUpcaster"/> chain over a stored event.
/// </summary>
/// <param name="upcasters">The upcasters.</param>
public class EventUpcastPipeline(IEnumerable<IEventUpcaster> upcasters)
{
    private readonly Dictionary<string, IEventUpcaster> _byType = upcasters.ToDictionary(u => u.FromType, StringComparer.Ordinal);

    /// <summary>
    /// Converts an event to its current shape.
    /// </summary>
    /// <param name="type">The stored type name.</param>
    /// <param name="payload">The stored payload.</param>
    /// <returns>The current type name and payload (unchanged when no upcaster applies).</returns>
    /// <exception cref="InvalidOperationException">The upcasters form a cycle.</exception>
    public (string Type, string Payload) Upcast(string type, string payload)
    {
        for (var steps = 0; steps <= _byType.Count; steps++)
        {
            if (!_byType.TryGetValue(type, out var upcaster))
                return (type, payload);
            (type, payload) = upcaster.Upcast(payload);
        }

        throw new InvalidOperationException($"The event upcasters starting at '{type}' form a cycle.");
    }
}
