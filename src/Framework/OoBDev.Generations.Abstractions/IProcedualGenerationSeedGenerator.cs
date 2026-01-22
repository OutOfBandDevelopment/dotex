using System;
using System.Reflection;

namespace OoBDev.Generations
{
    public interface IProcedualGenerationSeedGenerator
    {
        int Generate(IProcedualGenerationContext? context, int index, MethodBase method, object[] arguments);
        int Generate(IProcedualGenerationContext? context, int index, Type type);
    }
}
