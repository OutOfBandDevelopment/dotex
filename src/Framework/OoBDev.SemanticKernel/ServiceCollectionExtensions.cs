using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OoBDev.SemanticKernel;

/// <summary>
/// Provides extension methods for registering Semantic Kernel plugins and services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Semantic Kernel plugins and kernel services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddSemanticKernelPlugins(this IServiceCollection services)
    {
        services.AddKernelPlugIn<TimePlugIn>();
        services.AddKernelPlugIn<CurrentUserPlugIn>();

        services.TryAddKeyedTransient(KernelGlobal.Name, (sp, key) =>
        {
            var registeredPlugins = sp.GetServices<IKernelPlugIn>();

            var plugins = new KernelPluginCollection();
            foreach (var plugin in registeredPlugins)
            {
                plugins.AddFromObject(plugin);
            }

            var kernel = new Kernel(sp, plugins);

            return kernel;
        });

        services.TryAddKeyedTransient(KernelGlobal.Name, (sp, key) =>
            sp.GetRequiredKeyedService<Kernel>(key)
              .GetRequiredService<IChatCompletionService>()
            );

        return services;
    }
}
