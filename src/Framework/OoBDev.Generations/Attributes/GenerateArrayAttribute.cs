using OoBDev.Generations.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class GenerateArrayAttribute : Attribute, IGenerateObject
    {
        public static readonly int DefaultPriority = GenerateObjectAttribute.DefaultPriority;
        public virtual int Priority { get; set; } = DefaultPriority;
        public override object TypeId => this;

        public virtual bool CanGenerateValue(IProcedualGenerationContext context)
            => context.TargetType.IsArray
            ;

        //Note: this supports multidimensional arrays but it does not support any bounded other than 0 at this time
        public virtual object? GenerateValue(IProcedualGenerationContext context)
        {
            var rule = context.GetRule<ArrayAttribute>();

            static int product(IEnumerable<int> values) => values.Aggregate(1, (p, m) => p * m);

            var arrayElement = context.TargetType.GetElementType();
            var arrayRank = context.TargetType.GetArrayRank();

            var arrayLengths = new int[arrayRank];
            for (var l = 0; l < arrayRank; l++)
            {
                arrayLengths[l] = context.Random.Next(rule.MaximumLength - rule.MinimumLength) + rule.MinimumLength;
            }

            var array = Array.CreateInstance(arrayElement, arrayLengths);
            var arrayLength = product(arrayLengths);

            for (var i = 0; i < arrayLength; i++)
            {
                var elementContext = context.Provider.CreateContext(arrayElement, context.Attributes, context, i);
                var elementValue = context.Provider.Generate(elementContext);

                var indexes = new int[arrayRank];

                for (var r = 0; r < arrayRank; r++)
                {
                    indexes[r] = (i / product(arrayLengths.Take(r - 1))) % arrayLengths[r];
                }

                 array.SetValue(elementValue, indexes);
            }

            return array;
        }
    }
}
