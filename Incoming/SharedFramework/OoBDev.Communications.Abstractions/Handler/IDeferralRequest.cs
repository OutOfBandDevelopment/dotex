using System;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface IDeferralRequest
    {
        Guid CorrelationId { get; }
        string? ExtendedData { get; }
        DateTimeOffset HoldUntil { get; }
        string MessageType { get; }
        Guid TargetPersonId { get; }
    }
}