using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OoBDev.AspNetCore.Extensions;

public static class HostExtensions
{
    public static bool IsEnableSwagger(this IHost host) =>
        host.Services.GetRequiredService<IEnvironmentSettings>().EnableSwagger;
    public static bool IsHttpsRedirectionEnabled(this IHost host) =>
        host.Services.GetRequiredService<IEnvironmentSettings>().EnableHttpsRedirection;
}
