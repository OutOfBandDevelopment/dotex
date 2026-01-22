using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class GenerateInterfaceAttribute : Attribute, IGenerateObject
    {
        public static readonly int DefaultPriority = GenerateObjectAttribute.DefaultPriority - 1;
        public int Priority { get; set; } = DefaultPriority;
        public override object TypeId => this;

        public virtual bool CanGenerateValue(IProcedualGenerationContext context)
        {
            if (!context.TargetType.IsInterface) return false;
            else if (context.TargetType.IsGenericTypeDefinition) return false;
            else if (context.TargetType == typeof(IEnumerable)) return false;
            if (context.TargetType.IsGenericType)
            {
                if (new[] {
                     typeof(IEnumerable<>),
                     typeof(IList<>),
                     typeof(ICollection<>),
                     typeof(IDictionary<,>),
                     typeof(IQueryable<>),
                     typeof(IObservable<>)
                }.Contains(context.TargetType.GetGenericTypeDefinition()))
                    return false;

            }

            Debug.WriteLine($"Interface? >> {context.TargetType} << ");

            return true;
        }

        public object? GenerateValue(IProcedualGenerationContext context) =>
            context.Provider.DispatchFactory.Create(context)
            ;
}
}
