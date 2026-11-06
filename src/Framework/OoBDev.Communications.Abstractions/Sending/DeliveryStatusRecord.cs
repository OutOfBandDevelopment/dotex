using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>The stored delivery state of one message.</summary>
/// <param name="MessageId">The message id.</param>
/// <param name="ChannelName">The channel it was sent through.</param>
/// <param name="CorrelationId">The correlation id.</param>
/// <param name="Status">The current status.</param>
/// <param name="StatusDetails">Free text from the vendor or the failure.</param>
/// <param name="QueuedAt">When the record was created.</param>
/// <param name="SentAt">When the vendor accepted it.</param>
/// <param name="FailedAt">When it failed.</param>
public sealed record DeliveryStatusRecord(
    string MessageId,
    string ChannelName,
    string? CorrelationId,
    DeliveryStatus Status,
    string? StatusDetails,
    DateTimeOffset QueuedAt,
    DateTimeOffset? SentAt = null,
    DateTimeOffset? FailedAt = null);
