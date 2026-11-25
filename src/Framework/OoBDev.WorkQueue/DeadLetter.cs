using System;

namespace OoBDev.WorkQueue;

/// <summary>
/// The record left when an item leaves the flow.
/// </summary>
/// <param name="Stage">The stage.</param>
/// <param name="Key">The item key.</param>
/// <param name="Attempts">Attempts used.</param>
/// <param name="Reason">Why: <c>lease expired</c>, <c>attempts exhausted</c> or <c>non-retryable</c>.</param>
/// <param name="LastError">The last error text, when there was one.</param>
/// <param name="ClaimedBy">The last worker that held the item.</param>
/// <param name="DiedAt">When the item left the flow.</param>
/// <param name="RedrivenAt">When the item was returned to the flow, or null.</param>
public sealed record DeadLetter(string Stage, string Key, int Attempts, string Reason, string? LastError, string? ClaimedBy, DateTimeOffset DiedAt, DateTimeOffset? RedrivenAt);
