using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.Generations.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProcedualGenerationServices(
            this IServiceCollection services
            )
        {
            services.TryAddSingleton<IProcedualGenerationProviderBuilder, ProcedualGenerationProviderBuilder>();
            services.TryAddTransient(sp => sp.GetRequiredService<IProcedualGenerationProviderBuilder>().Build());
            return services;
        }
    }
}
