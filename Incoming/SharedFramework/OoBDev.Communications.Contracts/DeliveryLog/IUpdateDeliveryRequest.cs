using System;

namespace OoBDev.Communications.Contracts.DeliveryLog
{
    public interface IUpdateDeliveryRequest
    {
        DateTimeOffset Processed { get; }
        Guid RequestId { get; }
        string? ResultMessage { get; }
        bool Success { get; }
        string? TechnicalResultMessage { get; }
    }
}