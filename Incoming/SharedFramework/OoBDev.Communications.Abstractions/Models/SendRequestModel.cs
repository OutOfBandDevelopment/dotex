using OoBDev.Communications.Contracts.Handler;
using System;

namespace OoBDev.Communications.Abstractions.Models
{
    public class SendRequestModel : ISendRequest
    {
        public Guid TargetPersonId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string MessageType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object? Data { get; set; }
        public RequestPriorities Priority { get; set; } = RequestPriorities.Normal;
    }
}
