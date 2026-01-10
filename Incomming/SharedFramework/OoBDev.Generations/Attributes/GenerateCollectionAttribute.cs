using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateCollectionAttribute : GenerateArrayAttribute
    {
        public static readonly new int DefaultPriority = GenerateArrayAttribute.DefaultPriority - 1;

        public GenerateCollectionAttribute() => Priority = DefaultPriority;

        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            !context.TargetType.IsArray &&
            context.TargetType.IsGenericType &&
            !context.TargetType.IsGenericTypeDefinition &&
            !context.TargetType.IsPrimitive &&
            typeof(IEnumerable).IsAssignableFrom(context.TargetType) &&
            !typeof(IQueryable).IsAssignableFrom(context.TargetType)
            ;

        public override object? GenerateValue(IProcedualGenerationContext context)
        {
            var genericArguments = context.TargetType.GetGenericArguments();

            var emptyArray = genericArguments.Length switch
            {
                1 => Array.CreateInstance(genericArguments[0], 0),
                2 => Array.CreateInstance(typeof(KeyValuePair<,>).MakeGenericType(genericArguments), 0),
                _ => throw new NotSupportedException($"{genericArguments.Length}# of generic arguments not supported")
            };
            var arrayType = emptyArray.GetType();
            var _childContext = context.Provider.CreateContext(arrayType, context.Attributes, context, null);
            var arrayContent = (Array?)base.GenerateValue(_childContext);

            if (arrayContent == null) return null;

            var instance = GetInstance(context);

            var elementType = emptyArray.GetType().GetElementType();
            var addRange = instance.GetType().GetMethod("AddRange", new[] { typeof(IEnumerable<>).MakeGenericType(elementType) });
            if (addRange != null)
            {
                addRange.Invoke(instance, new[] { arrayContent });
                return instance;
            }
            else
            {
                var add = instance.GetType().GetMethod("Add", new[] { elementType });
                if (add != null)
                {
                    foreach (var item in arrayContent)
                    {
                        add.Invoke(instance, new[] { item });
                    }
                    return instance;
                }

                add = instance.GetType().GetMethod("TryAdd", elementType.GetGenericArguments());
                if (add != null)
                {
                    object[] getKvp(object o) => new[] {
                        elementType.GetProperty("Key").GetValue(o),
                        elementType.GetProperty("Value").GetValue(o)
                    };
                    foreach (var item in arrayContent)
                    {
                        add.Invoke(instance, getKvp(item));
                    }
                    return instance;
                }
            }

            throw new NotSupportedException($"{context.TargetType} is not supported");
        }

        private object GetInstance(IProcedualGenerationContext context)
        {
            if (!context.TargetType.IsInterface) return Activator.CreateInstance(context.TargetType);

            var mapped = new[]
            {
                (typeof(IDictionary<, >), typeof(Dictionary<, >)),
                (typeof(IList< >), typeof(List< >)),
                (typeof(IEnumerable< >), typeof(List< >)),
                (typeof(IQueryable< >), typeof(List< >)),
                (typeof(ICollection< >), typeof(Collection< >)),
            }.FirstOrDefault(i => i.Item1 == context.TargetType.GetGenericTypeDefinition());
            if (mapped.Item2 != null)
            {
                var genericType = mapped.Item2.MakeGenericType(context.TargetType.GetGenericArguments());
                var instance = Activator.CreateInstance(genericType);
                return instance;
            }

            throw new NotSupportedException($"{context.TargetType} is not supported");
        }
    }
}
