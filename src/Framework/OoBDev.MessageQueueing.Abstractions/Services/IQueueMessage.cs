using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;
/// <summary>
/// Represents a message within a message queue.
/// </summary>
public interface IQueueMessage
{
    /// <summary>
    /// Gets the content type of the message.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets the correlation identifier of the message.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the payload object of the message.
    /// </summary>
    object Payload { get; }

    /// <summary>
    /// Gets the type of the payload object.
    /// </summary>
    string? PayloadType { get; }

    /// <summary>
    /// Gets the properties associated with the message.
    /// </summary>
    Dictionary<string, object?> Properties { get; }
}
