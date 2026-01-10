using System;

namespace OoBDev.Communications.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommunicationAttribute : Attribute
    {
        public string? MessageType { get; set; }
        public RequestPriorities Priority { get; set; } = RequestPriorities.Normal;
}
}
