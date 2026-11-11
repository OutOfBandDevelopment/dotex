namespace OoBDev.EventSourcing;

/// <summary>
/// Converts a stored event of an older shape to the current one when it is read; the stored record is never changed.
/// </summary>
public interface IEventUpcaster
{
    /// <summary>
    /// Gets the stored event type name this upcaster converts.
    /// </summary>
    string FromType { get; }

    /// <summary>
    /// Converts the payload.
    /// </summary>
    /// <param name="payload">The stored JSON payload.</param>
    /// <returns>The current type name and JSON payload; the result can be upcast again by another upcaster.</returns>
    (string Type, string Payload) Upcast(string payload);
}
