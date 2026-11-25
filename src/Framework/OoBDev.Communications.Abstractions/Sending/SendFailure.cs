using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>Details of a failed send.</summary>
/// <param name="Kind">The failure class.</param>
/// <param name="Message">A message without message content.</param>
/// <param name="StatusCode">The vendor status code, when there is one.</param>
/// <param name="RetryAfter">The delay the vendor asked for, when there is one.</param>
public sealed record SendFailure(SendFailureKind Kind, string Message, int? StatusCode = null, TimeSpan? RetryAfter = null);
