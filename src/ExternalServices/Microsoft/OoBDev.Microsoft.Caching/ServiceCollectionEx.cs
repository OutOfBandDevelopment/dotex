using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Microsoft.Caching;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionEx
{
    public static IServiceCollection TryAddMicrosoftCachingServices(this IServiceCollection services) =>
        new MicrosoftCachingRegistrar().AddServices(services);
}
