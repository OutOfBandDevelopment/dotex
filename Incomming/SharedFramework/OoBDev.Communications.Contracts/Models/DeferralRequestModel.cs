using OoBDev.Communications.Contracts.Handler;
using System;

namespace OoBDev.Communications.Contracts.Models
{
    public class DeferralRequestModel : IDeferralRequest
    {
        public Guid CorrelationId { get; set; }
        public Guid TargetPersonId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string MessageType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? ExtendedData { get; set; }
        public DateTimeOffset HoldUntil { get; set; }
    }
}
