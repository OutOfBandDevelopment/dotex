using OoBDev.Caching.Common.Factories;
using OoBDev.Caching.Common.Managers;
using OoBDev.Caching.Contracts;
using OoBDev.ComponentModel.DependencyInjection;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: HasSharedFrameworkComponents]
namespace OoBDev.Caching.Common
{
    public class OoBDevCachingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<ICachingManager, CachingManager>();
            services.TryAddTransient<ICacheableFactory, CacheableFactory>();
            return services;
        }
    }
}
