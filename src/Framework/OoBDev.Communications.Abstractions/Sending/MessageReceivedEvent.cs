namespace OoBDev.Communications.Abstractions;

/// <summary>An inbound message.</summary>
/// <param name="MessageId">The message id.</param>
/// <param name="Message">The message.</param>
public sealed record MessageReceivedEvent(string MessageId, IMessage Message) : ChannelEvent(MessageId);
