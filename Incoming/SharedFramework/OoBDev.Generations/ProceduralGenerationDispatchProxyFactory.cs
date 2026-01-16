using System;

namespace OoBDev.Generations
{
    public class ProceduralGenerationDispatchProxyFactory : IProceduralGenerationDispatchProxyFactory
    {
        public object? Create(IProcedualGenerationContext context) =>
            context.TargetType.IsInterface ?
            ProceduralGenerationDispatchProxy.Create(context) :
            throw new NotSupportedException($"{context.TargetType} is not supported only interfaces may be dispatched")
            ;
    }
}
