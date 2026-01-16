using OoBDev.Generations.Rules;
using System;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateDoubleAttribute : GenerateNullableAttribute
    {
        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            new[] {
                typeof(double), typeof(double?),
                typeof(float), typeof(float?),
                typeof(decimal), typeof(decimal?),
            }.Contains(context.TargetType);

        protected override object? OnGenerateValue(IProcedualGenerationContext context)
        {
            var rule = context.GetRule<NumberAttribute>();

            var result = context.Random.NextDouble();

            if (rule.Factor != 1.0) result *= rule.Factor;
            if (rule.Percision >= 0) result = Math.Round(result, rule.Percision);

            if (context.TargetType == typeof(float) || context.TargetType == typeof(float?)) return (float)result;
            else if (context.TargetType == typeof(decimal) || context.TargetType == typeof(decimal?)) return (decimal)result;
            else return result;
        }
    }
}
