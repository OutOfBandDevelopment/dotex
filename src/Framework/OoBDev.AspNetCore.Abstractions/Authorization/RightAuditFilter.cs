using System;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>What to read from the right audit trail.</summary>
/// <param name="Subject">Only this caller, or all when <see langword="null"/>.</param>
/// <param name="Result">Only this outcome (a <see cref="RightCheckResult"/> name), or all when <see langword="null"/>.</param>
/// <param name="From">Only checks at or after this time.</param>
/// <param name="To">Only checks before this time.</param>
/// <param name="MaxCount">The most entries to return (at least 1).</param>
public sealed record RightAuditFilter(string? Subject = null, string? Result = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int MaxCount = 100);
