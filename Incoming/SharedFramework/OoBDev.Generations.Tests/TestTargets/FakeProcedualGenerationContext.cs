using System;
using System.Collections.Generic;

namespace OoBDev.Generations.Tests.TestTargets
{
    public class FakeProcedualGenerationContext : IProcedualGenerationContext
    {
#pragma warning disable CS8618

        public int Seed { get; set; }
        public Random Random { get; set; }
        public Type TargetType { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        public IProcedualGenerationContext Parent { get; set; }
        public IProcedualGenerationProvider Provider { get; set; }
        public IEnumerable<IProcedualGenerationContext> Children { get; set; }
#pragma warning restore CS8618
    }
}
