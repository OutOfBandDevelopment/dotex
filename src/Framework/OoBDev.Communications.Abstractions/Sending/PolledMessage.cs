namespace OoBDev.Communications.Abstractions;

/// <summary>An inbound message fetched by a poll, with the provider's token for acknowledging it.</summary>
/// <param name="Message">The message.</param>
/// <param name="Receipt">The provider token passed to <see cref="IPollableChannelProvider.AcknowledgeAsync"/>.</param>
public sealed record PolledMessage(IMessage Message, string Receipt);
