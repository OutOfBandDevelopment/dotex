using System;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface ISendRequest
    {
        Guid TargetPersonId { get; }
        string MessageType { get; }
        object? Data { get; }
        RequestPriorities Priority { get; }
    }
}
