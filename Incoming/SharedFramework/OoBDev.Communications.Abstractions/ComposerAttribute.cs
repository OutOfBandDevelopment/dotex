using System;

namespace OoBDev.Communications.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ComposerAttribute : Attribute
    {
        public string? DeliveryChannel { get; set; }
    }
}
