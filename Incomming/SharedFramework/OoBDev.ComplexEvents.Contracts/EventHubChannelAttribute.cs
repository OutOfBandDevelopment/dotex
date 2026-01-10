using System;

namespace OoBDev.ComplexEvents.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EventHubChannelAttribute : Attribute
    {
        public string? PartitionKey { get; set; }
        public string? EventHubName { get; set; }
    }
}
