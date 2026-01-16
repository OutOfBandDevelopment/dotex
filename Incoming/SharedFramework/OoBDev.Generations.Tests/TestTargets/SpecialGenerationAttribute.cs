using System;

namespace OoBDev.Generations.Tests.TestTargets
{
    [AttributeUsage(AttributeTargets.All)]
    public class SpecialGenerationAttribute : Attribute, IGenerateObject
    {
        public override object TypeId => this;
        public int Priority { get; }

        public bool CanGenerateValue(IProcedualGenerationContext context) => context.TargetType == typeof(string);

        public object GenerateValue(IProcedualGenerationContext context) => $"Seed: {context.Seed}";
    }
}
