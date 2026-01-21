using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddCensusGeocodingServices(this IServiceCollection services) =>
            new CensusGeocodingRegistrar().AddServices(services);
    }
}
