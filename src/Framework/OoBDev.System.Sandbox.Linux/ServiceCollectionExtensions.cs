using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System.Sandbox.Linux;

/// <summary>Registers the Linux sandbox launcher.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the Linux <see cref="ISandboxLauncher"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
    /// <returns>The service collection.</returns>
    [SupportedOSPlatform("linux")]
    public static IServiceCollection AddLinuxSandbox(this IServiceCollection services, string pluginAssembly)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISandboxLauncher>(new LinuxSandboxLauncher(pluginAssembly)));
        return services;
    }
}
