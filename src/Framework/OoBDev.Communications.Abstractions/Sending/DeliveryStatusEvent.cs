namespace OoBDev.Communications.Abstractions;

/// <summary>A delivery status change.</summary>
/// <param name="MessageId">The message id.</param>
/// <param name="Update">The change.</param>
public sealed record DeliveryStatusEvent(string MessageId, DeliveryStatusUpdate Update) : ChannelEvent(MessageId);
