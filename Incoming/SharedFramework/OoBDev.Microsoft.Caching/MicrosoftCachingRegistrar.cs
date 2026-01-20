using OoBDev.Microsoft.Caching.Providers;
using OoBDev.Caching.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Microsoft.Caching
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MicrosoftCachingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddSingleton<ICachingProvider, MicrosoftMemoryCachingProvider>();
            return services;
        }
    }
}
