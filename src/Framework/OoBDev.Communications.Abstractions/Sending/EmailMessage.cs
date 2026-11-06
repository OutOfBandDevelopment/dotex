using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>The plain implementation of <see cref="IEmailMessage"/>.</summary>
public sealed class EmailMessage : IEmailMessage
{
    /// <inheritdoc />
    public string MessageId { get; init; } = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <inheritdoc />
    public string[] To { get; init; } = [];

    /// <inheritdoc />
    public string[] Cc { get; init; } = [];

    /// <inheritdoc />
    public string[] Bcc { get; init; } = [];

    /// <inheritdoc />
    public string? From { get; init; }

    /// <inheritdoc />
    public string? ReplyTo { get; init; }

    /// <inheritdoc />
    public string Subject { get; init; } = string.Empty;

    /// <inheritdoc />
    public string? TextContent { get; init; }

    /// <inheritdoc />
    public string? HtmlContent { get; init; }
}
