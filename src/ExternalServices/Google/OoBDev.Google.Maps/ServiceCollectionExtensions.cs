using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.SpatialServices;

namespace OoBDev.Google.Maps;

/// <summary>Registers the Google geocoder.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>The provider key for configured selection.</summary>
    public const string ProviderKey = "google-maps";

    /// <summary>Adds <see cref="GoogleLocationServices"/> as <see cref="ILocationServices"/> with a typed <c>HttpClient</c>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the options, for example the API key; bind the <see cref="GoogleMapsOptions.SectionName"/> section in the composition root for configuration.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddGoogleGeocoding(this IServiceCollection services, Action<GoogleMapsOptions>? configure = null)
    {
        var options = services.AddOptions<GoogleMapsOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.AddHttpClient<GoogleLocationServices>(c => c.BaseAddress = GoogleLocationServices.DefaultBaseAddress);
        services.TryAddTransient<ILocationServices>(sp => sp.GetRequiredService<GoogleLocationServices>());
        return services;
    }
}
