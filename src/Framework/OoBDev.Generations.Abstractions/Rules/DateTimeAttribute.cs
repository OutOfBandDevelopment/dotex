using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class DateTimeAttribute : Attribute, IHavePriority
    {
        public string MinimumTime { get; set; } = TimeSpan.MinValue.ToString();
        public string MaximumTime { get; set; } = TimeSpan.MaxValue.ToString();
        public string MinimumDateTime { get; set; } = DateTime.MinValue.ToString();
        public string MaximumDateTime { get; set; } = DateTime.MaxValue.ToString();

        public int Priority { get; set; }
        public override object TypeId => this;
    }
}
