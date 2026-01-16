using System;

namespace OoBDev.Communications.Contracts.DeliveryLog
{
    public interface ICreateDeliveryRequest
    {
        DeliveryChannelType DeliveryChannel { get; }
        string? EnhancedData { get; }
        string? Html { get; }
        string MessageType { get; }
        Guid? NotificationId { get; }
        Guid PersonId { get; }
        DateTimeOffset Requested { get; }
        Guid RequestGroupId { get; }
        string SentTo { get; }
        string? Subject { get; }
        string? Text { get; }
    }
}