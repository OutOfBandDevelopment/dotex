using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.SemanticKernel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelPlugIn<T>(this IServiceCollection services) where T : class, IKernelPlugIn
    {
        services.AddTransient<IKernelPlugIn, T>();
        return services;
    }
}
