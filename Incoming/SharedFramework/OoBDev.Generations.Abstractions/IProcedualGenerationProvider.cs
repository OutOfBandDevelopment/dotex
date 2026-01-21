using System;
using System.Collections.Generic;
using System.Reflection;

namespace OoBDev.Generations
{
    public interface IProcedualGenerationProvider
    {
        IProcedualGenerationContext CreateContext(Type type, IEnumerable<Attribute>? attributes = default, IProcedualGenerationContext? context = default, int? index = default);
        IProcedualGenerationContext CreateContext(MethodBase method, object[]? args = default, IEnumerable<Attribute>? attributes = default, IProcedualGenerationContext? context = default, int? index = default);

        object? Generate(IProcedualGenerationContext context, bool required = true);

        IProceduralGenerationDispatchProxyFactory DispatchFactory { get; }
        IServiceProvider? ServiceProvider { get; }
        IProceduralGenerationTypeBuilderFactory TypeBuilder { get; }
    }
}
