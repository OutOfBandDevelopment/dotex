using System;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface IDeferralWaiting
    {
        Guid CorrelationId { get; }
        string ExtendedData { get; }
        string MessageType { get; }
        Guid NotificationDeferralId { get; }
        Guid TargetPersonId { get; }
    }
}