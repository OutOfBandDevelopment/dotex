using System;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>One right check, as stored in the audit trail.</summary>
/// <param name="At">When the check ran.</param>
/// <param name="Subject">The caller's id, or <c>anonymous</c>.</param>
/// <param name="Rights">The required rights, comma separated.</param>
/// <param name="Result">The outcome (<see cref="RightCheckResult"/> name).</param>
public sealed record RightAuditEntry(DateTimeOffset At, string Subject, string Rights, string Result);
