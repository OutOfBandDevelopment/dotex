using System;

namespace OoBDev.ComplexEvents.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EventHubChannelAttribute : Attribute
    {
        public string? PartitionKey { get; set; }
        public string? EventHubName { get; set; }
    }
}
