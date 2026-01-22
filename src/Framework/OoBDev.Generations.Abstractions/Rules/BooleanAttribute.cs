using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class BooleanAttribute : Attribute, IHavePriority
    {
        public int ModulusOdds { get; set; } = 2;
        public int Priority { get; set; } = int.MaxValue;
        public override object TypeId => this;
    }
}
