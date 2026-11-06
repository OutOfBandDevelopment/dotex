using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>A message that can be sent through a channel.</summary>
public interface IMessage
{
    /// <summary>Gets the message id; a repeated id on the same channel is delivered once.</summary>
    string MessageId { get; }

    /// <summary>Gets the id that ties messages of one request together.</summary>
    string? CorrelationId { get; }

    /// <summary>Gets when the message was created.</summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>Gets provider-neutral extra values.</summary>
    IDictionary<string, object> Metadata { get; }
}
