using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>The outcome of one orchestrated request.</summary>
/// <param name="CorrelationId">The id shared by the messages of the request.</param>
/// <param name="Outcome">What happened.</param>
/// <param name="Reason">Why the request was deferred or skipped; null when sent.</param>
/// <param name="Results">One send result per channel, empty unless sent.</param>
public sealed record CommunicationResult(
    string CorrelationId,
    CommunicationOutcome Outcome,
    string? Reason,
    IReadOnlyList<SendResult> Results);
