using System;

namespace OoBDev.Common.Logging;

/// <summary>
/// this is the payload to record with the audit log
/// </summary>
public record class AuditLogInfo
{
    /// <summary>
    /// Note: this should be HttpContext.Request.Path
    /// </summary>
    public required string ServiceCall { get; init; }

    /// <summary>
    /// Note: this should be requesting payload
    /// </summary>
    public object? Parameters { get; init; }

    /// <summary>
    /// response returned to client
    /// </summary>
    public object? Response { get; init; }

    /// <summary>
    /// current system time
    /// </summary>
    public required DateTimeOffset TimeStamp { get; init; }

    /// <summary>
    /// HTTP Request Id
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// HTTP Correlation ID
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// HTTP Response Code 
    /// </summary>
    public int StatusCode { get; init; }
}
