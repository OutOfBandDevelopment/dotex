using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Generations
{
    public static class ProcedualGenerationExtensions
    {
        public static T GetRule<T>(this IProcedualGenerationContext context) where T : Attribute, new() =>
            context.Attributes.OfType<T>().OrderBy(i => (i as IHavePriority)?.Priority ?? 0).FirstOrDefault() ?? new T();

        public static T Generate<T>(this IProcedualGenerationContext context) =>
            (T)context.Provider.Generate(context.Provider.CreateContext(typeof(T), default, context, default)) ??
            throw new NotSupportedException($"Unable to generate {typeof(T)}")
            ;

        public static T Generate<T>(this IProcedualGenerationProvider provider) =>
            (T)provider.Generate(provider.CreateContext(typeof(T), default, default, default)) ??
            throw new NotSupportedException($"Unable to generate {typeof(T)}")
            ;

        public static T Generate<T>(this IServiceProvider serviceProvider) =>
            (
                ((IProcedualGenerationProvider?)serviceProvider.GetService(typeof(IProcedualGenerationProvider))) ??
                throw new NotSupportedException($"Unable to resolve {typeof(IProcedualGenerationProvider)} ensure it is registered in your dependency injection container")
            ).Generate<T>();

        public static T ChooseFrom<T>(this IProcedualGenerationContext context, IEnumerable<T> items) =>
            items.ElementAt(context.Random.Next(items.Count()));
    }
}
