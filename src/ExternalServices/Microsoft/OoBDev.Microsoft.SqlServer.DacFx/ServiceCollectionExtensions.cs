using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DacFx;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDacPacCompilerServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDacpacMergeCompiler, DacpacMergeCompiler>();
        services.TryAddSingleton<IDacPacCompilerConfig, DacPacCompilerConfig>();

        services.TryAddSingleton<IDacPacMergeTemplateFactory, DacPacMergeTemplateFactory>();
        services.TryAddSingleton(sp => sp.GetRequiredService<IDacPacMergeTemplateFactory>().Create().GetAwaiter().GetResult());

        services.TryAddSingleton<IDacPacMergeDefinitionFactory, DacPacMergeDefinitionFactory>();
        services.TryAddSingleton(sp => sp.GetRequiredService<IDacPacMergeDefinitionFactory>().Create(sp.GetRequiredService<IDacPacMergeTemplate>()));

        return services;
    }
}
