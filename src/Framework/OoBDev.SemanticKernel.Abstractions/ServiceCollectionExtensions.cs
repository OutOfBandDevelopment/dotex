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
}
