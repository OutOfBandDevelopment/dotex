using System;
using System.Collections;

namespace OoBDev.Generations.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class GenerateAbstractAttribute : Attribute, IGenerateObject
    {
        public static readonly int DefaultPriority = GenerateObjectAttribute.DefaultPriority - 1;
        public int Priority { get; set; } = DefaultPriority;
        public override object TypeId => this;

        public virtual bool CanGenerateValue(IProcedualGenerationContext context) =>
            context.TargetType.IsAbstract &&
            !context.TargetType.IsGenericTypeDefinition &&
            context.TargetType != typeof(IEnumerable)
            ;

        public object? GenerateValue(IProcedualGenerationContext context) =>
            context.Provider.TypeBuilder.Create(context)
            ;
    }
}
