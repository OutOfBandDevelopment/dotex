using OoBDev.Communications.Contracts.DeliveryLog;
using System;

namespace OoBDev.Communications.Abstractions.Models
{
    public class UpdateDeliveryRequestModel : IUpdateDeliveryRequest
    {
        public Guid RequestId { get; set; }
        public DateTimeOffset Processed { get; set; }
        public bool Success { get; set; }
        public string? ResultMessage { get; set; }
        public string? TechnicalResultMessage { get; set; }
    }
}
