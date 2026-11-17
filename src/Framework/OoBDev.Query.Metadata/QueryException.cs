using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A request the registry does not allow. The message is safe to show the caller.
/// </summary>
/// <param name="message">What is wrong and what is allowed.</param>
/// <param name="status">The HTTP status a host should answer with.</param>
public sealed class QueryException(string message, int status = 400) : Exception(message)
{
    /// <summary>The HTTP status a host should answer with.</summary>
    public int Status { get; } = status;
}
