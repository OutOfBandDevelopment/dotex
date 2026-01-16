using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OoBDev.Generations
{
    public class ProcedualGenerationSeedGenerator : IProcedualGenerationSeedGenerator
    {
        public int Generate(IProcedualGenerationContext? context, int index, MethodBase method, object[] arguments) =>
            Seed(context, Basis(context, index, method, arguments));
        public int Generate(IProcedualGenerationContext? context, int index, Type type) =>
            Seed(context, Basis(context, index, type));

        internal int Seed(IProcedualGenerationContext? context, string input)
        {
            var basis = Encoding.UTF8.GetBytes(input ?? "");
            var paddedEncoding = basis.Concat(new byte[4 - (basis.Length % 4)]).ToArray();
            var seed = Enumerable.Range(0, paddedEncoding.Length / 4)
                                 .Select(i => BitConverter.ToInt32(paddedEncoding, i * 4))
                                 .Aggregate(0, (a, b) => a ^ b)
                                 ;

            Trace.WriteLine($"Seed: {input} => {seed}", "ProcedualGeneration-SeedGenerator");
            return seed;
        }

        public string TypeString(IProcedualGenerationContext? context, Type type) =>
            type.ToString();

        public string? ObjectString(IProcedualGenerationContext? context, object? value) =>
            value?.ToString();

        internal string Basis(IProcedualGenerationContext? context, int index, MethodBase method, object[] arguments) =>
            new
            {
                seed = context?.Seed ?? 0,
                index,
                FullName = TypeString(context, method.DeclaringType),
                method.Name,
                Arguments = string.Join(";", from i in Enumerable.Range(0, method.GetParameters().Length)
                                             let p = method.GetParameters()[i]
                                             let v = arguments.ElementAtOrDefault(i)
                                             select $"{p.Name} = {ObjectString(context, v)}"), //TODO: the object value here should capture the hierarchy
            }.ToString();

        internal string Basis(IProcedualGenerationContext? context, int index, Type type) =>
            new
            {
                seed = context?.Seed ?? 0,
                index,
                FullName = TypeString(context, type),
            }.ToString();
    }
}
