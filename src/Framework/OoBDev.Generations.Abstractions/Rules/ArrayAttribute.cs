using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]

    public class ArrayAttribute : Attribute, IHavePriority
    {
        public int MinimumLength { get; set; } = 1;
        public int MaximumLength { get; set; } = 5;
        public int Priority { get; set; } = int.MaxValue;
        public override object TypeId => this;
    }
}
