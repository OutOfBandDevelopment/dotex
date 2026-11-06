namespace OoBDev.Communications.Abstractions;

/// <summary>Something a provider reported through a webhook.</summary>
/// <param name="MessageId">The message id the event belongs to.</param>
public abstract record ChannelEvent(string MessageId);
