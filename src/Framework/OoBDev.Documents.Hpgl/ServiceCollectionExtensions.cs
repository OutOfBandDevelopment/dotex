using Microsoft.Extensions.DependencyInjection;
using OoBDev.Documents.Conversion;

namespace OoBDev.Documents.Hpgl;

/// <summary>Registers the HP-GL conversion.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the HP-GL to SVG <see cref="IDocumentConversionHandler"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddHpglConversion(this IServiceCollection services)
    {
        services.AddTransient<IDocumentConversionHandler, HpglToSvgConversionHandler>();
        return services;
    }
}
