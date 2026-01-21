using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class PhoneAttribute : Attribute, IGenerateObject
    {
        public int Priority { get; }

        public bool CanGenerateValue(IProcedualGenerationContext context) =>
            context.TargetType == typeof(string);

        public object? GenerateValue(IProcedualGenerationContext context) =>
            $"{context.Random.Next(100,999)}-555-{context.Random.Next(1000, 9999)}";
    }
}
