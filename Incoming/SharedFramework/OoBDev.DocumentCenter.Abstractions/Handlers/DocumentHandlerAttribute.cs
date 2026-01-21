using System;

namespace OoBDev.DocumentCenter.Abstractions.Handlers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DocumentHandlerAttribute : Attribute
    {
        public DocumentTypes InputType { get; set; }
        public DocumentTypes OutputType { get; set; }

        public int Priority { get; set; }

        public override object TypeId => this;
    }
}
