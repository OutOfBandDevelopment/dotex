using OoBDev.Google.Maps.SpatialServices;
using OoBDev.SpatialServices.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Google.Maps
{
    [ExcludeFromCodeCoverage]
    public class GoogleMapsRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<ILocationServiceClient, LocationServiceClient>();
            services.TryAddTransient<ILocationServiceMap, LocationServiceMap>();
            services.AddTransient<ILocationServices, GoogleMapsLocationServices>();
            return services;
        }
    }
}