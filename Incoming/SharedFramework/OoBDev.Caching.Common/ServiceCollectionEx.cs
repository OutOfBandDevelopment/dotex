using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Caching.Common
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddOoBDevCachingServices(this IServiceCollection services) =>
            new OoBDevCachingRegistrar().AddServices(services);
    }
}
