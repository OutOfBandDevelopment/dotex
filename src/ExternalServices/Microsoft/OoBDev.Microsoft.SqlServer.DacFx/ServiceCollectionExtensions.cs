using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DacFx;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Provides extension methods for registering DacPac compiler services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DacPac compiler services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
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
