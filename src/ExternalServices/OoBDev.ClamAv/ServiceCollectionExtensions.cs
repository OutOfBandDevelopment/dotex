using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Documents.Scanning;
using OoBDev.System;

namespace OoBDev.ClamAv;

/// <summary>Registers the ClamAV scanner.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ClamAvContentScanner"/> as the <see cref="IContentScanner"/> (and under the key <see cref="ClamAvGlobals.ProviderKey"/>).
    /// Options bind from the <c>ClamAv</c> section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddClamAvScanner(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<ClamAvOptions>(configuration, ClamAvOptions.SectionName);
        services.TryAddSingleton<ClamAvContentScanner>();
        services.TryAddSingleton<IContentScanner>(sp => sp.GetRequiredService<ClamAvContentScanner>());
        services.TryAddKeyedSingleton<IContentScanner>(ClamAvGlobals.ProviderKey, (sp, _) => sp.GetRequiredService<ClamAvContentScanner>());
        return services;
    }
}
