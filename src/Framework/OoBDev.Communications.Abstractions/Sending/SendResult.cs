using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>The outcome of a send.</summary>
/// <param name="Success">Whether the vendor accepted the message.</param>
/// <param name="MessageId">The message id.</param>
/// <param name="CorrelationId">The correlation id.</param>
/// <param name="ProviderMessageId">The id the vendor gave the message.</param>
/// <param name="Timestamp">When the outcome was known.</param>
/// <param name="Failure">Set when <paramref name="Success"/> is false.</param>
public sealed record SendResult(
    bool Success,
    string MessageId,
    string? CorrelationId,
    string? ProviderMessageId,
    DateTimeOffset Timestamp,
    SendFailure? Failure = null);
