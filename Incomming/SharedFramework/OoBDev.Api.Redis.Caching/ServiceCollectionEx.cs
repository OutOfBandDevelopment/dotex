using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Redis.Caching
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddRedisCachingServices(this IServiceCollection services) =>
            new RedisCachingRegistrar().AddServices(services);
    }
}
