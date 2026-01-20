using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Redis.Caching;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionEx
{
    public static IServiceCollection TryAddRedisCachingServices(this IServiceCollection services) =>
        new RedisCachingRegistrar().AddServices(services);
}
