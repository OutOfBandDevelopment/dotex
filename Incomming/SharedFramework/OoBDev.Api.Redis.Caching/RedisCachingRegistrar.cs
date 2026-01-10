using OoBDev.Api.Redis.Caching.Providers;
using OoBDev.Caching.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Api.Redis.Caching
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class RedisCachingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddTransient<ICachingProvider, RedisCachingProvider>();

            services.TryAddTransient<IConnectionMultiplexerFactory, ConnectionMultiplexerFactory>();
            return services;
        }
    }
}
