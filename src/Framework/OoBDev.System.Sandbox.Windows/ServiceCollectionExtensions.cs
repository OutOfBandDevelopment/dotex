using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>Registers the Windows sandbox launcher.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the Windows <see cref="ISandboxLauncher"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
    /// <returns>The service collection.</returns>
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddWindowsSandbox(this IServiceCollection services, string pluginAssembly)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISandboxLauncher>(new WindowsSandboxLauncher(pluginAssembly)));
        return services;
    }
}
