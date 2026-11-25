using System;

namespace OoBDev.MessageQueueing.Services;

/// <summary>A message waiting in the outbox to be published.</summary>
/// <param name="Id">The message id, also the key used to mark it sent.</param>
/// <param name="Message">The message to send.</param>
/// <param name="CreatedAt">When it was stored.</param>
public record OutboxEntry(string Id, object Message, DateTimeOffset CreatedAt);
