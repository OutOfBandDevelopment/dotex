using System;

namespace OoBDev.Communications.Contracts
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DataEnhancerAttribute : Attribute
    {
        public string? TargetedMessageType { get; set; }
        public int Priority { get; set; }

        public override object TypeId => this;
    }
}
