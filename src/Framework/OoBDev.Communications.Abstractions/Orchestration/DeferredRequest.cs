using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>A request held until a time.</summary>
/// <param name="Request">The held request, with its correlation id set.</param>
/// <param name="DueAt">When it may be released.</param>
public sealed record DeferredRequest(CommunicationRequest Request, DateTimeOffset DueAt);
