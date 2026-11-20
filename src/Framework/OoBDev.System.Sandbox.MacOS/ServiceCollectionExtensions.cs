using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System.Sandbox.MacOS;

/// <summary>Registers the MacOS sandbox launcher.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the MacOS <see cref="ISandboxLauncher"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
    /// <returns>The service collection.</returns>
    [SupportedOSPlatform("macos")]
    public static IServiceCollection AddMacOSSandbox(this IServiceCollection services, string pluginAssembly)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISandboxLauncher>(new MacOSSandboxLauncher(pluginAssembly)));
        return services;
    }
}
