using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OoBDev.SemanticKernel;

public static class ServiceCollectionExtensions
{
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
