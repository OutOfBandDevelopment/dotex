using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>A request to tell one target about something; channels, language and timing come from the target's preferences.</summary>
/// <param name="TargetId">Who to tell.</param>
/// <param name="MessageType">What happened, for example <c>order.confirmation</c>; composers choose by it.</param>
/// <param name="Data">Values for the message; data enhancers may add more.</param>
/// <param name="Priority">How urgent the request is.</param>
/// <param name="CorrelationId">Ties the messages of one request together; generated when null.</param>
public sealed record CommunicationRequest(
    string TargetId,
    string MessageType,
    IReadOnlyDictionary<string, object?> Data,
    RequestPriority Priority = RequestPriority.Normal,
    string? CorrelationId = null);
