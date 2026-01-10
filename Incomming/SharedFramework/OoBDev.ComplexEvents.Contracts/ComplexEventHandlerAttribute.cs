using System;

namespace OoBDev.ComplexEvents.Contracts
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ComplexEventHandlerAttribute : Attribute
    {
        public Type? TargetType { get; set; }

        public override object TypeId => this;
    }
}
