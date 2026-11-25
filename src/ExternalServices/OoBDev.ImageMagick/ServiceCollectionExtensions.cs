using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Media.Metadata.Raw;

namespace OoBDev.ImageMagick;

/// <summary>Registers the ImageMagick RAW developer.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="MagickRawDeveloper"/> as <see cref="IRawDeveloper"/> and under <see cref="ImageMagickGlobals.ProviderKey"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddMagickRawDeveloper(this IServiceCollection services)
    {
        services.TryAddSingleton<MagickRawDeveloper>();
        services.TryAddSingleton<IRawDeveloper>(sp => sp.GetRequiredService<MagickRawDeveloper>());
        services.TryAddKeyedSingleton<IRawDeveloper>(ImageMagickGlobals.ProviderKey, (sp, _) => sp.GetRequiredService<MagickRawDeveloper>());
        return services;
    }
}
