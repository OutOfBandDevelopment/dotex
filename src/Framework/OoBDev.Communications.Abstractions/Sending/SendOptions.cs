using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>Per-call overrides; <c>null</c> means "use the configured value".</summary>
public sealed class SendOptions
{
    /// <summary>Gets the correlation id to use when the message has none.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Gets how many retries follow the first attempt.</summary>
    public int? RetryCount { get; init; }

    /// <summary>Gets the delay before the first retry.</summary>
    public TimeSpan? RetryDelay { get; init; }

    /// <summary>Gets extra values passed along.</summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
