using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>The plain implementation of <see cref="ISmsMessage"/>.</summary>
public sealed class SmsMessage : ISmsMessage
{
    /// <inheritdoc />
    public string MessageId { get; init; } = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; init; } = TimeProvider.System.GetUtcNow();

    /// <inheritdoc />
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <inheritdoc />
    public string To { get; init; } = string.Empty;

    /// <inheritdoc />
    public string? From { get; init; }

    /// <inheritdoc />
    public string Body { get; init; } = string.Empty;
}
