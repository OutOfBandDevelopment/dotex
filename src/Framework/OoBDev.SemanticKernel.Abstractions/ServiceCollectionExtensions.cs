using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OoBDev.SemanticKernel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelPlugIn<T>(this IServiceCollection services) where T : class, IKernelPlugIn
    {
        services.AddTransient<IKernelPlugIn, T>();
        return services;
    }

    public static IServiceCollection AddKernelHostExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IModelNameAccessor, ModelNameAccessor>();

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
