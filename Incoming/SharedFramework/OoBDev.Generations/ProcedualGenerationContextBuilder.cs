using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.Generations
{
    public class ProcedualGenerationContextBuilder : IProcedualGenerationContextBuilder
    {
        public ProcedualGenerationContextBuilder(
            IProcedualGenerationSeedGenerator? seedGenerator = null
            )
        {
            SeedGenerator = seedGenerator ?? new ProcedualGenerationSeedGenerator();
        }

        public IProcedualGenerationSeedGenerator SeedGenerator { get; }

        public IProcedualGenerationContext CreateContext(
            IProcedualGenerationProvider provider,
            IProcedualGenerationContext? context,
            int index,
            Type type,
            IEnumerable<Attribute> attributes
            )
            => new ProcedualGenerationContext(
                seed: SeedGenerator.Generate(context, index, type),
                targetType: type,
                reference: type,
                parameters: Enumerable.Empty<object>(),
                attributes: attributes,
                parent: context,
                provider: provider
                );

        public IProcedualGenerationContext CreateContext(
            IProcedualGenerationProvider provider,
            IProcedualGenerationContext? context,
            int index,
            MethodBase method,
            object[] arguments,
            IEnumerable<Attribute> attributes
            )
            => new ProcedualGenerationContext(
                seed: SeedGenerator.Generate(context, index, method, arguments),
                targetType: method switch
                {
                    MethodInfo m => m.ReturnType,
                    ConstructorInfo c => c.DeclaringType,

                    _ => throw new NotSupportedException($"")
                },
                reference: method,
                parameters: arguments,
                attributes: method.GetCustomAttributes(),
                parent: context,
                provider: provider
                );
    }
}
