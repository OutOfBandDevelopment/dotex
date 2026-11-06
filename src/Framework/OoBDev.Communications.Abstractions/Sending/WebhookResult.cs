using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>The outcome of handling a webhook call.</summary>
/// <param name="Success">Whether the call was verified and processed.</param>
/// <param name="MessagesProcessed">How many events were dispatched.</param>
/// <param name="MessageIds">The ids of the dispatched events.</param>
/// <param name="ErrorMessage">Set when <paramref name="Success"/> is false.</param>
public sealed record WebhookResult(bool Success, int MessagesProcessed, IReadOnlyList<string> MessageIds, string? ErrorMessage = null);
