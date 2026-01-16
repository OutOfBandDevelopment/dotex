using OoBDev.Generations.Rules;
using System;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateBooleanAttribute : GenerateNullableAttribute
    {
        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            new[] {
                typeof(bool), typeof(bool?),
            }.Contains(context.TargetType);

        protected override object? OnGenerateValue(IProcedualGenerationContext context)
        {
            var rule = context.GetRule<BooleanAttribute>();
            return (context.Random.Next() % rule.ModulusOdds) == 0;
        }
    }
}
