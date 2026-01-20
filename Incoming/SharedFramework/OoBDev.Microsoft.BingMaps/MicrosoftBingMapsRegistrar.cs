using OoBDev.Microsoft.BingMaps.SpatialServices;
using OoBDev.SpatialServices.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Microsoft.BingMaps
{
    [ExcludeFromCodeCoverage]
    public class MicrosoftBingMapsRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IBingLocationServiceClient, BingLocationServiceClient>();
            services.TryAddTransient<IBingLocationRestClient, BingLocationRestClient>();
            services.TryAddTransient<IBingLocationServiceMap, BingLocationServiceMap>();
            services.AddTransient<ILocationServices, BingMapsLocationServices>();
            return services;
        }
    }
}