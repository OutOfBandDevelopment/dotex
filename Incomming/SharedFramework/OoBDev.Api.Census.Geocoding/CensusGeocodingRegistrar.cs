using OoBDev.Api.Census.Geocoding.SpatialServices;
using OoBDev.SpatialServices.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding
{
    [ExcludeFromCodeCoverage]
    public class CensusGeocodingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<ILocationServiceClient, LocationServiceClient>();
            services.TryAddTransient<ILocationServiceMap, LocationServiceMap>();
            services.AddTransient<ILocationServices, CensusGeocodingLocationServices>();
            services.TryAddTransient<ICensusGeocodingConfig, CensusGeocodingConfig>();
            return services;
        }
    }
}