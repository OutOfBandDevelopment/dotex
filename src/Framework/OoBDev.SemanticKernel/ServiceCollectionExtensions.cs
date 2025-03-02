using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.SemanticKernel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSemanticKernelPlugins(this IServiceCollection services)
    {
        services.AddKernelPlugIn<TimePlugIn>();
        services.AddKernelPlugIn<CurrentUserPlugIn>();
        return services;
    }
}
