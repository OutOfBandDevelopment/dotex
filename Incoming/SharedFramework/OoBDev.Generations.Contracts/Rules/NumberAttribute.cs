using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class NumberAttribute : Attribute, IHavePriority
    {
        //public double Minimum { get; set; } = double.NegativeInfinity;
        //public double Maximum { get; set; } = double.PositiveInfinity;

        public double Factor { get; set; } = 1.0;

        /// <summary>
        /// anything less than 0 means don't round
        /// </summary>
        public int Percision { get; set; } = -1;

        public int Priority { get; set; } = int.MaxValue;
        public override object TypeId => this;
    }
}
