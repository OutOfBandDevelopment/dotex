using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.SpatialServices;

namespace OoBDev.Census.Geocoding;

/// <summary>Registers the Census geocoder.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>The provider key for configured selection.</summary>
    public const string ProviderKey = "census";

    /// <summary>Adds <see cref="CensusLocationServices"/> as <see cref="ILocationServices"/> with a typed <c>HttpClient</c>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddCensusGeocoding(this IServiceCollection services)
    {
        services.AddHttpClient<CensusLocationServices>(c => c.BaseAddress = CensusLocationServices.DefaultBaseAddress);
        services.TryAddTransient<ILocationServices>(sp => sp.GetRequiredService<CensusLocationServices>());
        return services;
    }
}
