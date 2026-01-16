using System;

namespace OoBDev.Communications.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ComposerAttribute : Attribute
    {
        public string? DeliveryChannel { get; set; }
    }
}
