using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.Documents.Ocr;
using OoBDev.System;

namespace OoBDev.Docling;

/// <summary>Registers the Docling OCR client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DoclingOcr"/> as <see cref="IDocumentOcr"/> (and under <see cref="DoclingGlobals.ProviderKey"/>), binding <see cref="DoclingOptions"/> from <see cref="DoclingOptions.SectionName"/>.
    /// The HTTP timeout is the conversion timeout plus a minute; the client itself enforces the deadline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddDoclingOcr(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<DoclingOptions>(configuration, DoclingOptions.SectionName);
        services.AddHttpClient<DoclingOcr>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<DoclingOptions>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds + 60);
        });
        services.TryAddTransient<IDocumentOcr>(sp => sp.GetRequiredService<DoclingOcr>());
        services.TryAddKeyedTransient<IDocumentOcr>(DoclingGlobals.ProviderKey, (sp, _) => sp.GetRequiredService<DoclingOcr>());
        return services;
    }
}
