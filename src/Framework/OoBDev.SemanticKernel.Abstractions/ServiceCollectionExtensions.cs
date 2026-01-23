using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.SemanticKernel;

/// <summary>
/// Provides extension methods for registering Semantic Kernel plugins with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Semantic Kernel plugin implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The plugin implementation type that implements <see cref="IKernelPlugIn"/>.</typeparam>
    /// <param name="services">The service collection to add the plugin to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// The plugin is registered with a transient lifetime, meaning a new instance is created
    /// each time it is requested from the service provider.
    /// </remarks>
    public static IServiceCollection AddKernelPlugIn<T>(this IServiceCollection services) where T : class, IKernelPlugIn
    {
        services.AddTransient<IKernelPlugIn, T>();
        return services;
    }
}
