using System;

namespace OoBDev.Generations
{
    public class ProcedualGenerationProviderBuilder : IProcedualGenerationProviderBuilder
    {
        private IServiceProvider? _serviceProvider;

        public ProcedualGenerationProviderBuilder(IServiceProvider? serviceProvider = null) => _serviceProvider = serviceProvider;

        public IProcedualGenerationProvider Build() => new ProcedualGenerationProvider(_serviceProvider);
    }
}
