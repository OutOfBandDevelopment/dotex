using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>A change of the delivery status of one message.</summary>
/// <param name="MessageId">The message id.</param>
/// <param name="NewStatus">The status the vendor reported.</param>
/// <param name="StatusDetails">Free text from the vendor.</param>
/// <param name="Timestamp">When the vendor reported it.</param>
public sealed record DeliveryStatusUpdate(string MessageId, DeliveryStatus NewStatus, string? StatusDetails, DateTimeOffset Timestamp);
