using System;
using System.Collections.Generic;

namespace OoBDev.Generations
{
    public interface IProcedualGenerationContext
    {
        int Seed { get; }
        Random Random { get; }
        Type TargetType { get; }
        IEnumerable<Attribute> Attributes { get; }
        IProcedualGenerationContext? Parent { get; }
        IProcedualGenerationProvider Provider { get; }
        IEnumerable<IProcedualGenerationContext> Children { get; }
    }
}
