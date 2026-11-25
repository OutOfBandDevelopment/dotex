using System;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>One held communication request.</summary>
public sealed class DeferredRequestRow
{
    /// <summary>The key.</summary>
    public Guid Id { get; set; }

    /// <summary>UTC ticks when the request is due; indexed for the release query.</summary>
    public long DueAtTicks { get; set; }

    /// <summary>The target the request is for.</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>The message type, which names the templates.</summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>The <c>RequestPriority</c> as a number.</summary>
    public int Priority { get; set; }

    /// <summary>The correlation id, when the request had one.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>The request data as JSON.</summary>
    public string DataJson { get; set; } = "{}";
}
